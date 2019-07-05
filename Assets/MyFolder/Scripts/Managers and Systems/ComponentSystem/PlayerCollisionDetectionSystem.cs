//using Assets.MyFolder.Scripts.Basics;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using UnityEngine;

//namespace Assets.MyFolder.Scripts.Managers_and_Systems
//{
//    [UpdateAfter(typeof(StepPhysicsWorld)), AlwaysUpdateSystem]
//    public sealed class PlayerCollisionDetectionSystem : JobComponentSystem
//    {
//        private BuildPhysicsWorld _buildPhysicsWorld;
//        private StepPhysicsWorld _stepPhysicsWorld;

//        protected override void OnCreate()
//        {
//            _buildPhysicsWorld = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
//            _stepPhysicsWorld = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
//            GetEntityQuery(ComponentType.ReadOnly<UserIdSingleton>());
//        }

//        struct CollisionJob : ICollisionEventsJob
//        {
//            public ComponentDataFromEntity<DestroyableComponentData> Destroyable;
//            public void Execute(CollisionEvent ev)
//            {
//                if (Destroyable.Exists(ev.Entities.EntityA))
//                    Destroyable[ev.Entities.EntityA] = new DestroyableComponentData { ShouldDestroy = true };
//                if (Destroyable.Exists(ev.Entities.EntityB))
//                    Destroyable[ev.Entities.EntityB] = new DestroyableComponentData { ShouldDestroy = true };
//                Debug.Log(ev.Entities.EntityA + " : " + ev.Entities.EntityB);
//                Debug.Log(ev.AccumulatedImpulses);
//            }
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//#if UNITY_EDITOR
//            Debug.Log("");
//#endif
//            return new CollisionJob()
//            {
//                Destroyable = GetComponentDataFromEntity<DestroyableComponentData>()
//            }.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
//        }
//    }
//}
