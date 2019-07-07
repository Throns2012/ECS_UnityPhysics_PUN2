using System;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class ControlPlayerMachineRelativeToCameraSystem : ComponentSystem
    {
        public Camera MainCamera
        {
            get => _cameraTransform.GetComponent<Camera>();
            set => _cameraTransform = value.transform;
        }
        private Transform _cameraTransform;
        public IMoveNotifier Notifier;
        private float3 _bufferedVelocity;
        private long _nextIssueTicks;
        protected override void OnCreate()
        {
            FindComponentOfInterfaceOrClassHelper.FindComponentOfInterfaceOrClass(out Camera camera);
            MainCamera = camera;
            _nextIssueTicks = DateTime.Now.Ticks + TicksIntervalHelper.IntervalTicks;
            var c1 = new NativeArray<ComponentType>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<PlayerMachineTag>(),
            };
            GetEntityQuery(c1);
            c1.Dispose();
        }

        protected override void OnUpdate()
        {
            var current = DateTime.Now.Ticks;

            OrderWhenTimeComes(current);
            UpdateBufferedVelocity();
        }

        private void OrderWhenTimeComes(long current)
        {
            if (current < _nextIssueTicks) return;
            if (_bufferedVelocity.x != 0 || _bufferedVelocity.y != 0 || _bufferedVelocity.z != 0)
            {
                Notifier.OrderMoveCommand(_bufferedVelocity, new DateTime(_nextIssueTicks).AddMilliseconds(250));
            }
            _bufferedVelocity.x = _bufferedVelocity.y = _bufferedVelocity.z = 0;
            _nextIssueTicks = ((current / TicksIntervalHelper.IntervalTicks) + 1) * TicksIntervalHelper.IntervalTicks;
        }

        private void UpdateBufferedVelocity()
        {
            var keyW = Input.GetKey(KeyCode.W);
            var keyA = Input.GetKey(KeyCode.A);
            var keyS = Input.GetKey(KeyCode.S);
            var keyD = Input.GetKey(KeyCode.D);
            var keyShift = Input.GetKey(KeyCode.LeftShift);
            _bufferedVelocity += CalculateDeltaVelocity(keyW, keyS, keyA, keyD, keyShift);
        }

        private float3 CalculateDeltaVelocity(bool keyW, bool keyS, bool keyA, bool keyD, bool keyShift)
        {
            Vector3 delta = default;
            if (keyW || keyS)
            {
                var forward = _cameraTransform.forward;
                if (keyW)
                    delta += 10f * forward;
                if (keyS)
                    delta -= 10f * forward;
            }
            if (keyA || keyD)
            {
                var right = _cameraTransform.right;
                if (keyA)
                    delta -= 10f * right;
                if (keyD)
                    delta += 10f * right;
            }
            if (keyShift)
            {
                delta += 20f * _cameraTransform.up;
            }
            delta *= Time.deltaTime;
            return delta;
        }
    }
}
