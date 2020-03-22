using Unity.Entities;

namespace Elements.CameraBase.Datas
{
    [GenerateAuthoringComponent]
    public struct CameraBasePositionData : IComponentData
    {
        public float movementSpeed;
    }
}