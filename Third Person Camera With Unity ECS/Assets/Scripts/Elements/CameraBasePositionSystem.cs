using Elements.CameraBase.Datas;
using UnityEngine;
using Unity.Entities;

namespace Elements.CameraBase.Systems
{
    public class CameraBasePositionSystem : ComponentSystem
    {
        private Vector3 targetPosition;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            var targetObject = GameObject.FindGameObjectWithTag(
                    Metadatas.Tags.Elements.CameraTarget.ToString()
                );

            targetPosition = targetObject.transform.position;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Transform transform, ref CameraBasePositionData positionData) =>
            {
                var newPosition = Vector3.MoveTowards(
                        transform.position,
                        targetPosition,
                        positionData.movementSpeed * Time.DeltaTime
                    );

                transform.position = newPosition;
            });
        }
    }
}