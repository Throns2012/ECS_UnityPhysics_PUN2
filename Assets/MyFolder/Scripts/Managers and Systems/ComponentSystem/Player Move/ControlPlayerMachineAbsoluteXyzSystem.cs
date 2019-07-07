using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class ControlPlayerMachineAbsoluteXyzSystem : ComponentSystem
    {
        public IMoveNotifier Notifier;
        private float3 _bufferedVelocity;
        private long _nextIssueTicks;

        protected override void OnCreate()
        {
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

        private void UpdateBufferedVelocity()
        {
            var keyW = Input.GetKey(KeyCode.W);
            var keyA = Input.GetKey(KeyCode.A);
            var keyS = Input.GetKey(KeyCode.S);
            var keyD = Input.GetKey(KeyCode.D);
            var keyShift = Input.GetKey(KeyCode.LeftShift);
            _bufferedVelocity += CalculateDeltaVelocity(keyW, keyS, keyA, keyD, keyShift);
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

        private static float3 CalculateDeltaVelocity(bool keyW, bool keyS, bool keyA, bool keyD, bool keyShift)
        {
            float3 delta = default;
            if (keyW)
                delta.z += 10f;
            if (keyS)
                delta.z -= 10f;
            if (keyA)
                delta.x -= 10f;
            if (keyD)
                delta.x += 10f;
            if (keyShift)
                delta.y += 20f;
            delta *= Time.deltaTime;
            return delta;
        }
    }
}