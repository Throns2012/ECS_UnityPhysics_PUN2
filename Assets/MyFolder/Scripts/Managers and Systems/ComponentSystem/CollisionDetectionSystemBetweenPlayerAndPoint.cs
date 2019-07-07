using Assets.MyFolder.Scripts.Basics;
using System.Threading;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public sealed class CollisionDetectionSystemBetweenPlayerAndPoint : JobComponentSystem
    {
        private BuildPhysicsWorld _buildPhysicsWorld;
        private StepPhysicsWorld _stepPhysicsWorld;
        private UserPointManager _userPointManager;
        private Entity _playerEntity;
        private EntityQuery _query;
        public PhotonView View;

        protected override void OnCreate()
        {
            _buildPhysicsWorld = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            _stepPhysicsWorld = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
            _userPointManager = World.Active.GetOrCreateSystem<UserPointManager>();
            RequireForUpdate(_query = GetEntityQuery(ComponentType.ReadOnly<PlayerMachineTag>()));
            RequireForUpdate(GetEntityQuery(ComponentType.ReadWrite<DestroyableComponentData>(), ComponentType.ReadOnly<Point>()));
        }

        protected override void OnStartRunning()
        {
            var actor = View.OwnerActorNr;
            using (var array = _query.ToEntityArray(Allocator.TempJob))
            {
                for (var i = 0; i < array.Length; i++)
                {
                    if (EntityManager.GetComponentData<TeamTag>(array[i]).Id != actor) continue;
                    _playerEntity = array[i];
                    return;
                }
            }
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
                Interlocked.Add(ref *PointPtr, PointAccessor[point].Value);
            }
        }

        protected override unsafe JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!EntityManager.Exists(_playerEntity)) return inputDeps;
            return new CollisionJob()
            {
                DestroyableAccessor = GetComponentDataFromEntity<DestroyableComponentData>(),
                PointAccessor = GetComponentDataFromEntity<Point>(true),
                PointPtr = _userPointManager.PointPtr,
                PlayerEntity = _playerEntity,
            }.Schedule(
                _stepPhysicsWorld.Simulation,
                ref _buildPhysicsWorld.PhysicsWorld,
                JobHandle.CombineDependencies(_stepPhysicsWorld.FinalSimulationJobHandle, _buildPhysicsWorld.FinalJobHandle, inputDeps));
        }
    }
}
