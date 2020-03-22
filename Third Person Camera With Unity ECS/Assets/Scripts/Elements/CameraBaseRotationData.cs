using UnityEngine;
using Unity.Entities;

namespace Elements.CameraBase.Datas
{
    [GenerateAuthoringComponent]
    public struct CameraBaseRotationData : IComponentData
    {
        [HideInInspector] public const float RotZ = 0f;
        [HideInInspector] public float rotX;
        [HideInInspector] public float rotY;

        public float maxClampAngle;
        public float minClampAngle;
        public float inputSensitivity;
    }
}