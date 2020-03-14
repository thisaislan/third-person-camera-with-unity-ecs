using System.Collections.Generic;

namespace Unity.Properties.Tests
{
    public struct MyOwnVector
    {
        public float x;
        public float y;
        [Property] public float z { get; set; }
    }
    
    public struct MyOwnVectorWithFormerlySerializedAs
    {
        [UnityEngine.Serialization.FormerlySerializedAs("x")]
        public float RenamedX;
        [FormerlySerializedAs("y")]
        public float RenamedY;
        [FormerlySerializedAs("z")]
        [Property] public float RenamedZ { get; set; }
    }
    
    public class FormerlySerializedAsMockData
    {
        public float MyFloat;
        public List<int> SomeList;
        public MyOwnVector MyVector;
        public string SomeString { get; set; }
    }
    
    public class FormerlySerializedAsData
    {
        [UnityEngine.Serialization.FormerlySerializedAs("MyFloat")]
        public float SomeSimpleFloat = 0;

        [FormerlySerializedAs("TryToFoolYou")]
        [FormerlySerializedAs("SomeList")]
        public List<int> ListOfInts = new List<int>( );
        
        [FormerlySerializedAs("MyVector")]
        public MyOwnVectorWithFormerlySerializedAs MyVectorRenamed;
    }
}
