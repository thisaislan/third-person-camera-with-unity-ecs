using UnityEngine;
using Unity.Entities;

namespace Elements.CameraBase.Datas
{
    [GenerateAuthoringComponent]
    public struct CameraBaseInputData : IComponentData
    {
        [HideInInspector] public float inputX;
        [HideInInspector] public float inputZ;
    }
}