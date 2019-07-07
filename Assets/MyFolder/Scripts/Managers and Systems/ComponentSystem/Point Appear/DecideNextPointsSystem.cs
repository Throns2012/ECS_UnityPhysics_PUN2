using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class DecideNextPointsSystem : ComponentSystem
    {
        private const long Interval = TicksIntervalHelper.IntervalTicks * 4L;
        private long _nextTicks;
        private int _appearCount = 64;
        private Job job;
        public ref float3 Min => ref job.Min;
        public ref float3 Max => ref job.Max;
        public ref int PointMin => ref job.PointMin;
        public ref int PointMax => ref job.PointMax;
        public IPointAppearNotifier Notifier;

        [BurstCompile]
        struct Job : IJobParallelFor
        {
            public NativeArray<Translation> Translations;
            public NativeArray<Point> Points;
            public float3 Min, Max;
            public int PointMin, PointMax;
            public Random Random;
            public void Execute(int index)
            {
                Random.InitState((uint)(Random.state + index));
                Translations[index] = new Translation() { Value = Random.NextFloat3(Min, Max) };
                Points[index] = new Point(Random.NextInt(PointMin, PointMax));
            }
        }

        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<Prefab>(), ComponentType.ReadWrite<Point>(), ComponentType.ReadWrite<Translation>(), ComponentType.ReadOnly<Disabled>()));
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<PlayerMachineTag>()));
            job = new Job
            {
                Points = new NativeArray<Point>(_appearCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                Translations = new NativeArray<Translation>(_appearCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                Random = new Random((uint)DateTime.Now.Ticks),
            };
        }

        protected override void OnDestroy()
        {
            job.Translations.Dispose();
            job.Points.Dispose();
        }

        protected override void OnUpdate()
        {
            var current = DateTime.Now.Ticks;
            if (current < _nextTicks) return;
            _nextTicks = ((current / Interval) + 1) * Interval;

            job.Random.InitState((uint)current ^ (uint)(current >> 32));
            job.Schedule(_appearCount, 256).Complete();
            Notifier.NextPoint(job.Translations, job.Points, new DateTime(_nextTicks + Interval * 2));
        }
    }
}
