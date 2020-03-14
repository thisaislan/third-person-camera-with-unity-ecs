#if !NET_DOTS

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// Types of supported generic JSON objects.
    /// </summary>
    public enum JsonDataType
    {
        /// <summary>
        /// <code>null</code> value
        /// </summary>
        Null,
        /// <summary>
        /// Map of member names to values
        /// </summary>
        Object,
        /// <summary>
        /// List of value
        /// </summary>
        Array,
        /// <summary>
        /// Numeric value
        /// </summary>
        Number,
        /// <summary>
        /// String value
        /// </summary>
        String,
        /// <summary>
        /// Boolean value
        /// </summary>
        Boolean
    }

    /// <summary>
    /// Generic, mutable data structure used to represent JavaScript Objects.
    /// </summary>
    /// <remarks>
    /// Useful when you need to modify free-form JSON documents not represented by a C# type known at compile time.
    ///
    /// For performance reasons, using <see cref="JsonObject"/> is not recommended when you de-serialize JSON into
    /// strongly-typed instances. For example, if you only need to read (not write) free-form JSON, use
    /// <see cref="SerializedObjectReader"/> directly.
    ///
    /// The <see cref="JsonObject"/> is compatible with read-only visitors (see <see cref="Unity.Properties.IPropertyVisitor"/>).
    /// </remarks>
    public sealed class JsonObject
    {
        Dictionary<string, JsonObject> m_ObjectData;
        List<JsonObject> m_ArrayData;
        string m_StringData;
        double m_NumberData;
        bool m_BooleanData;
        JsonDataType m_DataType;

        static JsonObject()
        {
            PropertyBagResolver.Register(new JsonObjectPropertyBag());
        }

        /// <summary>
        /// Creates a Null object.
        /// </summary>
        public JsonObject()
        {
        }

        /// <summary>
        /// Creates an object of the given type.
        /// </summary>
        /// <param name="dataType">Object type</param>
        public JsonObject(JsonDataType dataType)
        {
            ChangeType(dataType);
        }

        /// <summary>
        /// Creates a mutable object from the given immutable serialized object view.
        /// </summary>
        /// <param name="view">Immutable view from which this constructor initializes this instance.</param>
        public JsonObject(SerializedObjectView view)
        {
            ChangeType(JsonDataType.Object);
            foreach (var member in view)
            {
                this[member.Name().ToString()] = new JsonObject(member.Value());
            }
        }

        /// <summary>
        /// Creates a mutable object from the given immutable serialized value view.
        /// </summary>
        /// <param name="view">Immutable view from which this constructor initializes this instance.</param>
        public JsonObject(SerializedValueView view)
        {
            switch (view.Type)
            {
                case TokenType.Object:
                    ChangeType(JsonDataType.Object);
                    foreach (var member in view.AsObjectView())
                    {
                        this[member.Name().ToString()] = new JsonObject(member.Value());
                    }

                    break;
                case TokenType.Array:
                    ChangeType(JsonDataType.Array);
                    foreach (var element in view.AsArrayView())
                    {
                        m_ArrayData.Add(new JsonObject(element));
                    }

                    break;
                case TokenType.String:
                    ChangeType(JsonDataType.String);
                    m_StringData = view.AsStringView().ToString();
                    break;
                case TokenType.Primitive:
                    var primitive = view.AsPrimitiveView();
                    if (primitive.IsBoolean())
                    {
                        ChangeType(JsonDataType.Boolean);
                        m_BooleanData = primitive.AsBoolean();
                    }
                    else if (primitive.IsIntegral() || primitive.IsDecimal() || primitive.IsInfinity() || primitive.IsNaN())
                    {
                        ChangeType(JsonDataType.Number);
                        m_NumberData = primitive.AsDouble();
                    }
                    // else: null -> remain undefined
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a String object.
        /// </summary>
        /// <param name="value">Initial string value.</param>
        public JsonObject(string value)
        {
            m_DataType = JsonDataType.String;
            m_StringData = value;
        }

        /// <summary>
        /// Creates a Number object.
        /// </summary>
        /// <param name="value">Initial numeric value.</param>
        public JsonObject(double value)
        {
            m_DataType = JsonDataType.Number;
            m_NumberData = value;
        }

        /// <summary>
        /// Creates a Boolean object.
        /// </summary>
        /// <param name="value">Initial boolean value.</param>
        public JsonObject(bool value)
        {
            m_DataType = JsonDataType.Boolean;
            m_BooleanData = value;
        }

        /// <summary>
        /// Implicit conversion from <see cref="String"/> to <see cref="JsonObject"/>.
        /// </summary>
        public static implicit operator JsonObject(string rhs)
        {
            return new JsonObject(rhs);
        }
        
        /// <summary>
        /// Implicit conversion from <see cref="Double"/> to <see cref="JsonObject"/>.
        /// </summary>
        public static implicit operator JsonObject(double rhs)
        {
            return new JsonObject(rhs);
        }
        
        /// <summary>
        /// Implicit conversion from <see cref="Boolean"/> to <see cref="JsonObject"/>.
        /// </summary>
        public static implicit operator JsonObject(bool rhs)
        {
            return new JsonObject(rhs);
        }

        /// <summary>
        /// Current data type.
        /// </summary>
        public JsonDataType DataType => m_DataType;

        void CheckType(JsonDataType type)
        {
            if (m_DataType != type)
                throw new InvalidOperationException($"JSON object of type '{m_DataType}' found, but expected '{type}'");
        }

        /// <summary>
        /// Changes this object's data type.
        /// When the type changes, the underlying data resets.
        /// </summary>
        /// <param name="newType">New object data type.</param>
        /// <returns>True if the type changed, False otherwise.</returns>
        public bool ChangeType(JsonDataType newType)
        {
            if (newType == m_DataType)
                return false;

            m_ObjectData = null;
            m_ArrayData = null;
            m_StringData = null;
            m_NumberData = 0.0;
            m_BooleanData = false;

            switch (newType)
            {
                case JsonDataType.Object:
                    m_ObjectData = new Dictionary<string, JsonObject>();
                    break;
                case JsonDataType.Array:
                    m_ArrayData = new List<JsonObject>();
                    break;
            }

            m_DataType = newType;
            return true;
        }

        /// <summary>
        /// Gets or sets an object member by name.
        /// </summary>
        /// <param name="key">The member name.</param>
        /// <remarks>This indexer is only valid on objects of type <see cref="JsonDataType.Object"/>.</remarks>
        public JsonObject this[string key]
        {
            get
            {
                CheckType(JsonDataType.Object);
                return m_ObjectData[key];
            }
            set
            {
                CheckType(JsonDataType.Object);
                m_ObjectData[key] = value ?? new JsonObject();
            }
        }

        /// <summary>
        /// Gets or sets an array object by index.
        /// </summary>
        /// <param name="key">The member index.</param>
        /// <remarks>This indexer is only valid on objects of type <see cref="JsonDataType.Array"/>.</remarks>
        public JsonObject this[int key]
        {
            get
            {
                CheckType(JsonDataType.Array);
                return m_ArrayData[key];
            }
            set
            {
                CheckType(JsonDataType.Array);

                // pad
                for (var i = m_ArrayData.Count; i < key; ++i)
                    m_ArrayData.Add(new JsonObject());

                value = value ?? new JsonObject();

                // append or set
                if (key == m_ArrayData.Count)
                    m_ArrayData.Add(value);
                else
                    m_ArrayData[key] = value;
            }
        }

        /// <summary>
        /// Attempts to retrieve the object member value by name.
        /// </summary>
        /// <param name="key">The member name.</param>
        /// <param name="value">The found value, or null.</param>
        /// <returns>True if the value was found and returned, False otherwise.</returns>
        /// <remarks>This method is only valid on objects of type <see cref="JsonDataType.Object"/>.</remarks>
        public bool TryGetValue(string key, out JsonObject value)
        {
            CheckType(JsonDataType.Object);
            return m_ObjectData.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes a member from the current object.
        /// </summary>
        /// <param name="key">The name of the member to remove.</param>
        /// <returns>True if the object was found and removed, False otherwise.</returns>
        /// <remarks>This method is only valid on objects of type <see cref="JsonDataType.Object"/>.</remarks>
        public bool Remove(string key)
        {
            CheckType(JsonDataType.Object);
            return m_ObjectData.Remove(key);
        }

        /// <summary>
        /// Gets or sets the object string value.
        /// </summary>
        /// <remarks>This property is only valid on objects of type <see cref="JsonDataType.String"/>.</remarks>
        public string StringValue
        {
            get
            {
                CheckType(JsonDataType.String);
                return m_StringData;
            }
            set
            {
                CheckType(JsonDataType.String);
                m_StringData = value;
            }
        }

        /// <summary>
        /// Gets or sets the object numeric value.
        /// </summary>
        /// <remarks>This property is only valid on objects of type <see cref="JsonDataType.Number"/>.</remarks>
        public double NumberValue
        {
            get
            {
                CheckType(JsonDataType.Number);
                return m_NumberData;
            }
            set
            {
                CheckType(JsonDataType.Number);
                m_NumberData = value;
            }
        }

        /// <summary>
        /// Gets or sets the object boolean value.
        /// </summary>
        /// <remarks>This property is only valid on objects of type <see cref="JsonDataType.Boolean"/>.</remarks>
        public bool BooleanValue
        {
            get
            {
                CheckType(JsonDataType.Boolean);
                return m_BooleanData;
            }
            set
            {
                CheckType(JsonDataType.Boolean);
                m_BooleanData = value;
            }
        }

        /// <summary>
        /// Creates and returns an object from the given JSON string.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <returns>The created object.</returns>
        public static JsonObject DeserializeFromString(string json)
        {
            using (var memory = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            using (var reader = new SerializedObjectReader(memory))
            {
                return new JsonObject(reader.ReadObject());
            }
        }

        /// <summary>
        /// Serializes this object to a JSON string.
        /// </summary>
        /// <param name="visitor">Optional visitor used for serialization.</param>
        /// <returns>The created JSON string.</returns>
        public string Serialize(JsonVisitor visitor = null)
        {
            return JsonSerialization.Serialize(this, visitor);
        }
        
        #region Properties

        class JsonObjectPropertyBag : PropertyBag<JsonObject>
        {
            struct NullProperty : IProperty<JsonObject, string>
            {
                readonly string m_Name;
                readonly IPropertyAttributeCollection m_Attributes;

                public NullProperty(string name, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => false;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public string GetValue(ref JsonObject container) => null;
                public void SetValue(ref JsonObject container, string value) => throw new NotSupportedException();
            }
            
            struct ObjectProperty : IProperty<JsonObject, JsonObject>
            {
                readonly string m_Name;
                readonly JsonObject m_Value;
                readonly IPropertyAttributeCollection m_Attributes;

                public ObjectProperty(string name, JsonObject value, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Value = value;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => true;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public JsonObject GetValue(ref JsonObject container) => m_Value;
                public void SetValue(ref JsonObject container, JsonObject value) => throw new NotSupportedException();
            }

            struct ArrayProperty : ICollectionProperty<JsonObject, JsonObject>
            {
                struct ElementProperty<TElementValue> : ICollectionElementProperty<JsonObject, TElementValue>
                {
                    readonly int m_Index;
                    readonly TElementValue m_Value;
                    readonly IPropertyAttributeCollection m_Attributes;

                    public ElementProperty(int index, TElementValue value, bool isContainer, IPropertyAttributeCollection attributes = null)
                    {
                        m_Value = value;
                        m_Attributes = attributes;
                        m_Index = index;
                        IsContainer = isContainer;
                    }

                    public string GetName() => $"[{Index}]";
                    public int Index => m_Index;
                    public bool IsReadOnly => true;
                    public bool IsContainer { get; }
                    public IPropertyAttributeCollection Attributes => m_Attributes;
                    public TElementValue GetValue(ref JsonObject container) => m_Value;
                    public void SetValue(ref JsonObject container, TElementValue value) => throw new InvalidOperationException("Property is ReadOnly");
                }

                readonly string m_Name;
                readonly JsonObject m_Value;
                readonly IPropertyAttributeCollection m_Attributes;

                public ArrayProperty(string name, JsonObject value, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Value = value;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => false;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public JsonObject GetValue(ref JsonObject container) => m_Value;
                public void SetValue(ref JsonObject container, JsonObject value) => throw new InvalidOperationException("Property is ReadOnly");
                public int GetCount(ref JsonObject container) => m_Value.m_ArrayData.Count;
                public void SetCount(ref JsonObject container, int count) => throw new InvalidOperationException("Property is ReadOnly");
                public void Clear(ref JsonObject container) => throw new InvalidOperationException("Property is ReadOnly");

                public void GetPropertyAtIndex<TGetter>(ref JsonObject container, int index, ref ChangeTracker changeTracker, ref TGetter getter)
                    where TGetter : ICollectionElementPropertyGetter<JsonObject>
                {
                    var element = m_Value.m_ArrayData[index];
                    switch (element.DataType)
                    {
                        case JsonDataType.Null:
                            getter.VisitProperty<ElementProperty<string>, string>(
                                new ElementProperty<string>(
                                    index, null, false, m_Attributes), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Object:
                            getter.VisitProperty<ElementProperty<JsonObject>, JsonObject>(
                                new ElementProperty<JsonObject>(
                                    index, element, true, m_Attributes), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Array:
                            throw new NotImplementedException("Nested collections are not implemented");
                        case JsonDataType.String:
                            getter.VisitProperty<ElementProperty<string>, string>(
                                new ElementProperty<string>(
                                    index, element.StringValue, false, m_Attributes), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Number:
                            getter.VisitProperty<ElementProperty<double>, double>(
                                new ElementProperty<double>(
                                    index, element.NumberValue, false, m_Attributes), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Boolean:
                            getter.VisitProperty<ElementProperty<bool>, bool>(
                                new ElementProperty<bool>(
                                    index, element.BooleanValue, false, m_Attributes), ref container, ref changeTracker);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            struct StringProperty : IProperty<JsonObject, string>
            {
                readonly string m_Name;
                readonly JsonObject m_Value;
                readonly IPropertyAttributeCollection m_Attributes;

                public StringProperty(string name, JsonObject value, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Value = value;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => false;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public string GetValue(ref JsonObject container) => m_Value.StringValue;
                public void SetValue(ref JsonObject container, string value) => throw new NotSupportedException();
            }

            struct NumberProperty : IProperty<JsonObject, double>
            {
                readonly string m_Name;
                readonly JsonObject m_Value;
                readonly IPropertyAttributeCollection m_Attributes;

                public NumberProperty(string name, JsonObject value, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Value = value;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => false;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public double GetValue(ref JsonObject container) => m_Value.NumberValue;
                public void SetValue(ref JsonObject container, double value) => throw new NotSupportedException();
            }

            struct BooleanProperty : IProperty<JsonObject, bool>
            {
                readonly string m_Name;
                readonly JsonObject m_Value;
                readonly IPropertyAttributeCollection m_Attributes;

                public BooleanProperty(string name, JsonObject value, IPropertyAttributeCollection attributes = null)
                {
                    m_Name = name;
                    m_Value = value;
                    m_Attributes = attributes;
                }

                public string GetName() => m_Name;
                public bool IsReadOnly => true;
                public bool IsContainer => false;
                public IPropertyAttributeCollection Attributes => m_Attributes;
                public bool GetValue(ref JsonObject container) => m_Value.BooleanValue;
                public void SetValue(ref JsonObject container, bool value) => throw new NotSupportedException();
            }

            public override void Accept<TVisitor>(ref JsonObject container, ref TVisitor visitor, ref ChangeTracker changeTracker)
            {
                if (container.m_ObjectData == null)
                    return;

                foreach (var kvp in container.m_ObjectData)
                {
                    var name = kvp.Key;
                    var value = kvp.Value;
                    
                    switch (value.DataType)
                    {
                        case JsonDataType.Null:
                            visitor.VisitProperty<NullProperty, JsonObject, string>(new NullProperty(name), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Object:
                            visitor.VisitProperty<ObjectProperty, JsonObject, JsonObject>(new ObjectProperty(name, value), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Array:
                            visitor.VisitCollectionProperty<ArrayProperty, JsonObject, JsonObject>(new ArrayProperty(name, value), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Number:
                            visitor.VisitProperty<NumberProperty, JsonObject, double>(new NumberProperty(name, value), ref container, ref changeTracker);
                            break;
                        case JsonDataType.String:
                            visitor.VisitProperty<StringProperty, JsonObject, string>(new StringProperty(name, value), ref container, ref changeTracker);
                            break;
                        case JsonDataType.Boolean:
                            visitor.VisitProperty<BooleanProperty, JsonObject, bool>(new BooleanProperty(name, value), ref container, ref changeTracker);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public override bool FindProperty<TAction>(string name, ref JsonObject container, ref ChangeTracker changeTracker, ref TAction action)
            {
                if (container.m_ObjectData == null)
                    return false;

                if (!container.m_ObjectData.TryGetValue(name, out var value))
                    return false;
                
                switch (value.DataType)
                {
                    case JsonDataType.Null:
                        action.VisitProperty<NullProperty, string>(new NullProperty(name), ref container, ref changeTracker);
                        break;
                    case JsonDataType.Object:
                        action.VisitProperty<ObjectProperty, JsonObject>(new ObjectProperty(name, value), ref container, ref changeTracker);
                        break;
                    case JsonDataType.Array:
                        action.VisitCollectionProperty<ArrayProperty, JsonObject>(new ArrayProperty(name, value), ref container, ref changeTracker);
                        break;
                    case JsonDataType.Number:
                        action.VisitProperty<NumberProperty, double>(new NumberProperty(name, value), ref container, ref changeTracker);
                        break;
                    case JsonDataType.String:
                        action.VisitProperty<StringProperty, string>(new StringProperty(name, value), ref container, ref changeTracker);
                        break;
                    case JsonDataType.Boolean:
                        action.VisitProperty<BooleanProperty, bool>(new BooleanProperty(name, value), ref container, ref changeTracker);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }
        }
        
        #endregion // Properties
    }
}

#endif // !NET_DOTS