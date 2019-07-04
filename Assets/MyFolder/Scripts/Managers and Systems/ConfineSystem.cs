using Assets.MyFolder.Scripts.Basics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class ConfineSystem : JobComponentSystem
    {
        public float3 Min
        {
            get => _destroyJob.Min;
            set => _destroyJob.Min = value;
        }

        public float3 Max
        {
            get => _destroyJob.Max;
            set => _destroyJob.Max = value;
        }

        private DestroyJob _destroyJob;

        protected override void OnCreate()
        {
            _destroyJob = new DestroyJob();
        }

        [BurstCompile]
        struct DestroyJob : IJobForEach<DestroyableComponentData, Translation>
        {
            public float3 Min, Max;
            public void Execute([WriteOnly]ref DestroyableComponentData destroyable, [ReadOnly]ref Translation translation)
            {
                if (math.any(translation.Value < Min | translation.Value > Max))
                    destroyable.ShouldDestroy = true;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) => _destroyJob.Schedule(this, inputDeps);
    }
}
