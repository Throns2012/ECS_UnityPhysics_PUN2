using Assets.MyFolder.Scripts.Basics.CameraManipulation;
using Unity.Entities;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    /// <summary>
    /// 率直に言いまして、これの操作性は最悪と言えるでしょう。
    /// </summary>
    [UpdateAfter(typeof(FollowingCameraSystem))]
    public sealed class ManualCameraSystem : ComponentSystem
    {
        public Camera MainCamera
        {
            get => _mainCamera;
            set
            {
                _mainCamera = value;
                if (_mainCamera == null) return;
                _mainCameraTransform = _mainCamera.transform;
            }
        }

        public float MoveConstant = 20f, RotateConstant = 45f;
        private Transform _mainCameraTransform;
        private Camera _mainCamera;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ManualCameraControlSingleton>();
        }

        protected override void OnUpdate()
        {
            if (_mainCamera == null) return;
            var keyMap = GetSingleton<ManualCameraControlSingleton>();
            var delta = Time.deltaTime;
            if (keyMap.MoveAny)
            {
                var move = new Vector3();
                if (Input.GetKey(keyMap.MoveXAxisPlus))
                {
                    move.x += MoveConstant;
                }
                if (Input.GetKey(keyMap.MoveXAxisMinus))
                {
                    move.x -= MoveConstant;
                }
                if (Input.GetKey(keyMap.MoveYAxisPlus))
                {
                    move.y += MoveConstant;
                }
                if (Input.GetKey(keyMap.MoveYAxisMinus))
                {
                    move.y -= MoveConstant;
                }
                if (Input.GetKey(keyMap.MoveZAxisPlus))
                {
                    move.z += MoveConstant;
                }
                if (Input.GetKey(keyMap.MoveZAxisMinus))
                {
                    move.z -= MoveConstant;
                }
                _mainCameraTransform.position += _mainCameraTransform.rotation * (delta * move);
            }
            if (keyMap.RotateAny)
            {
                var rotate = new Vector3();
                if (Input.GetKey(keyMap.RotateXAxisPlus))
                {
                    rotate.x += RotateConstant;
                }
                if (Input.GetKey(keyMap.RotateXAxisMinus))
                {
                    rotate.x -= RotateConstant;
                }
                if (Input.GetKey(keyMap.RotateYAxisPlus))
                {
                    rotate.y += RotateConstant;
                }
                if (Input.GetKey(keyMap.RotateYAxisMinus))
                {
                    rotate.y -= RotateConstant;
                }
                if (Input.GetKey(keyMap.RotateZAxisPlus))
                {
                    rotate.z += RotateConstant;
                }
                if (Input.GetKey(keyMap.RotateZAxisMinus))
                {
                    rotate.z -= RotateConstant;
                }
                _mainCameraTransform.Rotate(delta * rotate);
            }
        }
    }
}
