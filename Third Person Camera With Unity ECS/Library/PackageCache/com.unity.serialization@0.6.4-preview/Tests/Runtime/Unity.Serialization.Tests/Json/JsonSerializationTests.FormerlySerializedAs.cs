using NUnit.Framework;
using Unity.Properties;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests
    {
        const string OriginalStructContainerAssemblyTypeName = "Some.Other.Assembly.StructContainer, Some.Other.Assembly";
        const string OriginalClassContainerAssemblyTypeName = "Some.Other.Assembly.ClassContainer, Some.Other.Assembly";

        interface IDataContainer { }

#pragma warning disable 649
        [FormerlySerializedAs(OriginalStructContainerAssemblyTypeName)]
        struct RenamedStructContainer : IDataContainer
        {
            public int Value;
        }

        struct StructContainerWithInterfaceMember
        {
            public IDataContainer Data;
        }

        [FormerlySerializedAs(OriginalClassContainerAssemblyTypeName)]
        class RenamedClassContainer : IDataContainer
        {
            public int Value;
        }

        class ClassContainerWithInterfaceMember
        {
            public IDataContainer Data;
        }
#pragma warning restore 649

        [Test]
        public void JsonSerialization_Serialize_FormerlySerializedAs()
        {
            var srcStructJson = $"{{ \"Data\": {{ \"$type\": \"{OriginalStructContainerAssemblyTypeName}\", \"Value\": 1 }} }}";
            var srcClassJson = $"{{ \"Data\": {{ \"$type\": \"{OriginalClassContainerAssemblyTypeName}\", \"Value\": 2 }} }}";

            var dstStructContainer = new StructContainerWithInterfaceMember();
            var dstClassContainer = new ClassContainerWithInterfaceMember();

            using (var result = JsonSerialization.DeserializeFromString(srcStructJson, ref dstStructContainer))
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(dstStructContainer.Data, Is.Not.Null);
                Assert.That(dstStructContainer.Data is RenamedStructContainer, Is.True);
                Assert.That(((RenamedStructContainer)dstStructContainer.Data).Value, Is.EqualTo(1));
            }

            using (var result = JsonSerialization.DeserializeFromString(srcClassJson, ref dstClassContainer))
            {
                Assert.That(result.Succeeded, Is.True);
                Assert.That(dstClassContainer.Data, Is.Not.Null);
                Assert.That(dstClassContainer.Data is RenamedClassContainer, Is.True);
                Assert.That(((RenamedClassContainer)dstClassContainer.Data).Value, Is.EqualTo(2));
            }
        }
    }
}
