using Elements.CameraBase.Datas;
using Extensions;
using UnityEngine;
using Unity.Entities;
using Axes = Metadatas.Input.Axes;

namespace Elements.CameraBase.Systems
{
    public class CameraBaseInputSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((ref CameraBaseInputData inputData) =>
            {
                var stickInputX = Axes.RightStickX.GetAxisRaw();
                var mouseX = Axes.MouseX.GetAxisRaw();
                inputData.inputX = stickInputX + mouseX;

                var stickIInputZ = Axes.RightStickY.GetAxisRaw();
                var mouseY = Axes.MouseY.GetAxisRaw();
                inputData.inputZ = stickIInputZ + mouseY;
            });
        }
    }
}