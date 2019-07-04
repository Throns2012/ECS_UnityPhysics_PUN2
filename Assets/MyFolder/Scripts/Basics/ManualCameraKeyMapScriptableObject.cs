using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Basics.CameraManipulation
{
    public struct ManualCameraControlSingleton : IComponentData
    {
        public KeyCode MoveXAxisPlus;
        public KeyCode MoveXAxisMinus;
        public KeyCode MoveYAxisPlus;
        public KeyCode MoveYAxisMinus;
        public KeyCode MoveZAxisPlus;
        public KeyCode MoveZAxisMinus;

        public bool MoveAny => Input.GetKey(MoveXAxisMinus) || Input.GetKey(MoveXAxisPlus) || Input.GetKey(MoveYAxisMinus) || Input.GetKey(MoveYAxisPlus) || Input.GetKey(MoveZAxisMinus) || Input.GetKey(MoveZAxisPlus);

        public KeyCode RotateXAxisPlus;
        public KeyCode RotateXAxisMinus;
        public KeyCode RotateYAxisPlus;
        public KeyCode RotateYAxisMinus;
        public KeyCode RotateZAxisPlus;
        public KeyCode RotateZAxisMinus;

        public bool RotateAny => Input.GetKey(RotateXAxisMinus) || Input.GetKey(RotateXAxisPlus) || Input.GetKey(RotateYAxisMinus) || Input.GetKey(RotateYAxisPlus) || Input.GetKey(RotateZAxisMinus) || Input.GetKey(RotateZAxisPlus);

        public static implicit operator ManualCameraControlSingleton(ManualCameraKeyMapScriptableObject obj)
        {
            return new ManualCameraControlSingleton()
            {
                MoveXAxisPlus = obj.MoveXAxisPlus,
                MoveXAxisMinus = obj.MoveXAxisMinus,
                MoveYAxisPlus = obj.MoveYAxisPlus,
                MoveYAxisMinus = obj.MoveYAxisMinus,
                MoveZAxisPlus = obj.MoveZAxisPlus,
                MoveZAxisMinus = obj.MoveZAxisMinus,
                RotateXAxisPlus = obj.RotateXAxisPlus,
                RotateXAxisMinus = obj.RotateXAxisMinus,
                RotateYAxisPlus = obj.RotateYAxisPlus,
                RotateYAxisMinus = obj.RotateYAxisMinus,
                RotateZAxisPlus = obj.RotateZAxisPlus,
                RotateZAxisMinus = obj.RotateZAxisMinus,
            };
        }
    }

    public sealed class ManualCameraKeyMapScriptableObject : ScriptableObject
    {
        public KeyCode MoveXAxisPlus;
        public KeyCode MoveXAxisMinus;
        public KeyCode MoveYAxisPlus;
        public KeyCode MoveYAxisMinus;
        public KeyCode MoveZAxisPlus;
        public KeyCode MoveZAxisMinus;
        public KeyCode RotateXAxisPlus;
        public KeyCode RotateXAxisMinus;
        public KeyCode RotateYAxisPlus;
        public KeyCode RotateYAxisMinus;
        public KeyCode RotateZAxisPlus;
        public KeyCode RotateZAxisMinus;
    }
}
