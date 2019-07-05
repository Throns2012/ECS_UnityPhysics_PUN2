using System;
using Assets.MyFolder.Scripts.Basics;
using Assets.MyFolder.Scripts.Utility;
using Assets.MyFolder.Scriptsics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class ControlPlayerMachineSystem : ComponentSystem
    {
        public IMoveNotifier Notifier;
        private float3 _bufferedVelocity;
        private long _nextIssueTicks;

        protected override void OnCreate()
        {
            _nextIssueTicks = DateTime.Now.Ticks + TicksIntervalHelper.IntervalTicks;
            var c1 = ArrayPool.Get<ComponentType>(1);
            c1[0] = ComponentType.ReadOnly<PlayerMachineTag>();
            GetEntityQuery(c1);
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

        private static bool NoInput(out bool keyW, out bool keyA, out bool keyS, out bool keyD, out bool keyShift)
        {
            keyW = Input.GetKey(KeyCode.W) | Input.GetKey(KeyCode.UpArrow);
            keyA = Input.GetKey(KeyCode.A) | Input.GetKey(KeyCode.LeftArrow);
            keyS = Input.GetKey(KeyCode.S) | Input.GetKey(KeyCode.DownArrow);
            keyD = Input.GetKey(KeyCode.D) | Input.GetKey(KeyCode.RightArrow);
            keyShift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
            return !keyW && !keyA && !keyS && !keyD && !keyShift;
        }
    }
}