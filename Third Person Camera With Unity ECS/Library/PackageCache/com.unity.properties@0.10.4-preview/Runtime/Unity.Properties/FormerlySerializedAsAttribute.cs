using System;

namespace Unity.Properties
{
    /// <summary>
    /// Use this attribute to rename a struct, class, field or property without losing its serialized value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class FormerlySerializedAsAttribute : Attribute
    {
        public string OldName { get; }

        public FormerlySerializedAsAttribute(string oldName)
        {
            OldName = oldName;
        }
    }
}
