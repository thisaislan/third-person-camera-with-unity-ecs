using Elements.CameraBase.Datas;
using UnityEngine;
using Unity.Entities;

namespace Elements.CameraBase.Systems
{
    public class CameraBaseRotationSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            Entities.ForEach((Transform transform, ref CameraBaseRotationData rotationData) =>
            {
                var rot = transform.localRotation.eulerAngles;
                rotationData.rotY = rot.y;
                rotationData.rotX = rot.x;
            });
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((
                Transform transform,
                ref CameraBaseRotationData rotationData,
                ref CameraBaseInputData inputData) =>
            {
                rotationData.rotY += inputData.inputX * rotationData.inputSensitivity * deltaTime;
                rotationData.rotX += inputData.inputZ * rotationData.inputSensitivity * deltaTime;

                rotationData.rotX = Mathf.Clamp(
                        rotationData.rotX,
                        rotationData.minClampAngle,
                        rotationData.maxClampAngle
                    );

                transform.rotation = Quaternion.Euler(
                        rotationData.rotX,
                        rotationData.rotY,
                        CameraBaseRotationData.RotZ
                    );
            });
        }
    }
}