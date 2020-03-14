#if !NET_DOTS

using System;
using NUnit.Framework;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    public class JsonObjectTests
    {
        [Test]
        public void JsonObject_Primitives()
        {
            var obj = new JsonObject();
            Assert.AreEqual(JsonDataType.Null, obj.DataType);
            
            obj = new JsonObject(42.0);
            Assert.AreEqual(JsonDataType.Number, obj.DataType);
            Assert.AreEqual(42.0, obj.NumberValue);

            obj = 123.0;
            Assert.AreEqual(123.0, obj.NumberValue);
            
            obj = new JsonObject(true);
            Assert.AreEqual(JsonDataType.Boolean, obj.DataType);
            Assert.AreEqual(true, obj.BooleanValue);

            obj = false;
            Assert.AreEqual(false, obj.BooleanValue);
            
            obj = new JsonObject("hello");
            Assert.AreEqual(JsonDataType.String, obj.DataType);
            Assert.AreEqual("hello", obj.StringValue);

            obj = "world";
            Assert.AreEqual("world", obj.StringValue);
        }
        
        [Test]
        public void JsonObject_Arrays()
        {
            var obj = new JsonObject(JsonDataType.Array);
            obj[0] = 1.0;
            obj[1] = "two";
            
            Assert.AreEqual(1.0, obj[0].NumberValue);
            Assert.AreEqual("two", obj[1].StringValue);
            
            // test padding in the indexer

            obj[3] = 3.0;
            
            Assert.AreEqual(JsonDataType.Null, obj[2].DataType);
            Assert.AreEqual(3.0, obj[3].NumberValue);
            
            // test adding in the indexer

            obj[4] = 4.0;
            Assert.AreEqual(4.0, obj[4].NumberValue);
        }
        
        [Test]
        public void JsonObject_Objects()
        {
            var obj = new JsonObject(JsonDataType.Object);
            obj["one"] = 1.0;
            obj["two"] = 2.0;

            obj["two"].NumberValue += 1.0;
            
            Assert.AreEqual(1.0, obj["one"].NumberValue);
            Assert.AreEqual(3.0, obj["two"].NumberValue);
            
            // test replace

            obj["two"] = "replaced";
            Assert.AreEqual("replaced", obj["two"].StringValue);
        }
        
        [Test]
        public void JsonObject_ChangeType()
        {
            JsonObject obj = "hello";
            Assert.AreEqual(JsonDataType.String, obj.DataType);

            obj.ChangeType(JsonDataType.Boolean);
            Assert.AreEqual(JsonDataType.Boolean, obj.DataType);
            Assert.AreEqual(false, obj.BooleanValue);

            obj = true;
            Assert.AreEqual(true, obj.BooleanValue);

            // switching types must reset storage
            obj.ChangeType(JsonDataType.String);
            obj.ChangeType(JsonDataType.Boolean);
            
            Assert.AreEqual(false, obj.BooleanValue);
            obj = true;

            // changing to the same type should not reset the current value
            obj.ChangeType(JsonDataType.Boolean);
            Assert.AreEqual(true, obj.BooleanValue);
        }
        
        const string k_JsonData = @"{
    ""hello"": ""world"",
    ""nested"": {
        ""num_value"": 123.0
    },
    ""collection"": [
        1, 2, 3, {
            ""bool_value"": true
        }
    ],
    ""some_secret"": ""!!secret!!"",
    ""remove_me"": ""!!removed!!""
}";

        [Test]
        public void JsonObject_Deserialize()
        {
            var obj = JsonObject.DeserializeFromString(k_JsonData);
            Assert.AreEqual("world", obj["hello"].StringValue);
            Assert.AreEqual(123.0, obj["nested"]["num_value"].NumberValue);
            Assert.AreEqual(3.0, obj["collection"][2].NumberValue);
            Assert.AreEqual(true, obj["collection"][3]["bool_value"].BooleanValue);
        }
        
        [Test]
        public void JsonObject_Deserialize_And_Serialize()
        {
            var obj = JsonObject.DeserializeFromString(k_JsonData);
            obj["hello"].StringValue = "!!replaced!!";
            obj.Remove("remove_me");

            var jsonString = obj.Serialize();
            Assert.IsTrue(jsonString.Contains("!!replaced!!"));
            Assert.IsFalse(jsonString.Contains("!!removed!!"));
            Assert.IsTrue(jsonString.Contains("!!secret!!"));
        }
        
        [Test]
        public void JsonObject_Deserialize_Nested_Collections()
        {
            const string jsonData = @"{
    ""collection"": [
        1, 2, 3, [4,5,6]
    ]
}";
            var obj = JsonObject.DeserializeFromString(jsonData);
            Assert.AreEqual(JsonDataType.Array, obj["collection"][3].DataType);
            Assert.AreEqual(6.0, obj["collection"][3][2].NumberValue);

            // TODO: fix this once nested collections are implemented in Properties
            Assert.Throws<NotImplementedException>(() => { obj.Serialize(); });
        }
    }
}

#endif // !NET_DOTS