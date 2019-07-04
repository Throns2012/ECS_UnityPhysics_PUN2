using Assets.MyFolder.Scripts.Basics;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public sealed class CollisionDetectionSystemBetweenPlayerAndPoint : JobComponentSystem
    {
        private BuildPhysicsWorld _buildPhysicsWorld;
        private StepPhysicsWorld _stepPhysicsWorld;
        private UserPointManager _userPointManager;

        protected override void OnCreate()
        {
            _buildPhysicsWorld = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            _stepPhysicsWorld = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
            _userPointManager = World.Active.GetOrCreateSystem<UserPointManager>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<PlayerMachineTag>()));
            RequireForUpdate(GetEntityQuery(ComponentType.ReadWrite<DestroyableComponentData>(), ComponentType.ReadOnly<Point>()));
        }

        [BurstCompile]
        unsafe struct CollisionJob : ICollisionEventsJob
        {
            [ReadOnly] public Entity PlayerEntity;
            [NativeDisableUnsafePtrRestriction] public int* PointPtr;
            public ComponentDataFromEntity<DestroyableComponentData> DestroyableAccessor;
            [ReadOnly] public ComponentDataFromEntity<Point> PointAccessor;
            public void Execute(CollisionEvent ev)
            {
                Entity point;
                if (PlayerEntity == ev.Entities.EntityA)
                    point = ev.Entities.EntityB;
                else if (PlayerEntity == ev.Entities.EntityB)
                    point = ev.Entities.EntityA;
                else return;
                if (!DestroyableAccessor.Exists(point) || !PointAccessor.Exists(point) || DestroyableAccessor[point].ShouldDestroy)
                    return;
                DestroyableAccessor[point] = new DestroyableComponentData() { ShouldDestroy = true };
#if UNITY_WEBGL && !UNITY_EDITOR
                *PointPtr += PointAccessor[point].Value;
#else
                Interlocked.Add(ref *PointPtr, PointAccessor[point].Value);
#endif
            }
        }

        protected override unsafe JobHandle OnUpdate(JobHandle inputDeps)
        {
            new CollisionJob()
            {
                DestroyableAccessor = GetComponentDataFromEntity<DestroyableComponentData>(),
                PointAccessor = GetComponentDataFromEntity<Point>(true),
                PointPtr = _userPointManager.PointPtr,
                PlayerEntity = GetSingleton<UserIdSingleton>().UserMachineEntity,
            }.Schedule(
                _stepPhysicsWorld.Simulation,
                ref _buildPhysicsWorld.PhysicsWorld,
                JobHandle.CombineDependencies(_stepPhysicsWorld.FinalSimulationJobHandle, _buildPhysicsWorld.FinalJobHandle)).Complete();
            return inputDeps;
        }
    }
}
