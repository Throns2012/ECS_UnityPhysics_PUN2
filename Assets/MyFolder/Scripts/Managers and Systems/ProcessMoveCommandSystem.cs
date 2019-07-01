using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Physics;

namespace Assets.MyFolder.Scripts
{
    public sealed class ProcessMoveCommandSystem : ComponentSystem
    {
        private EntityQuery _query;
        private EntityQuery _queryMoveCommand;

        protected override void OnCreate()
        {
            _queryMoveCommand = GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<MoveCommand>(),
            });
            _query = GetEntityQuery(new[]
            {
                ComponentType.ReadOnly<TeamTag>(),
                ComponentType.ReadWrite<PhysicsVelocity>(),
            });
            RequireForUpdate(_query);
            RequireForUpdate(_queryMoveCommand);
        }

        protected override unsafe void OnUpdate()
        {
            var typeMoveCommand = GetArchetypeChunkComponentType<MoveCommand>(true);
            var typeTeamTag = GetArchetypeChunkComponentType<TeamTag>(true);
            var typePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>();
            using (var moveCommandChunks = _queryMoveCommand.CreateArchetypeChunkArray(Allocator.TempJob))
            using (var velocitiesChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var moveCommandChunk in moveCommandChunks)
                {
                    foreach (var moveCommand in moveCommandChunk.GetNativeArray(typeMoveCommand))
                    {
                        foreach (var velocitiesChunk in velocitiesChunks)
                        {
                            var teamTags = velocitiesChunk.GetNativeArray(typeTeamTag);
                            var physicsVelocities = velocitiesChunk.GetNativeArray(typePhysicsVelocity);
                            for (var i = 0; i < teamTags.Length; i++)
                            {
                                if (teamTags[i].Id != moveCommand.Id) continue;
                                ref var physicsVelocity = ref UnsafeUtilityEx.ArrayElementAsRef<PhysicsVelocity>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(physicsVelocities), i);
                                physicsVelocity.Linear += moveCommand.DeltaVelocity;
                            }
                        }
                    }
                }
            }
            EntityManager.DestroyEntity(_queryMoveCommand);
        }
    }
}
