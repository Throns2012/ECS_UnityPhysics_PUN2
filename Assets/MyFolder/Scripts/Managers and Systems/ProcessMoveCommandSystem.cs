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
                ComponentType.ReadWrite<DestroyableComponentData>(),
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
            var typeDestroyable = GetArchetypeChunkComponentType<DestroyableComponentData>();
            var typeTeamTag = GetArchetypeChunkComponentType<TeamTag>(true);
            var typePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>();
            using (var moveCommandChunks = _queryMoveCommand.CreateArchetypeChunkArray(Allocator.TempJob))
            using (var velocitiesChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var moveCommandChunk in moveCommandChunks)
                {
                    var moveCommands = moveCommandChunk.GetNativeArray(typeMoveCommand);
                    var destroyables = moveCommandChunk.GetNativeArray(typeDestroyable);

                    for (var i = 0; i < moveCommands.Length; i++)
                    {
                        if (destroyables[i].ShouldDestroy) continue;
                        destroyables[i] = new DestroyableComponentData() { ShouldDestroy = true };
                        foreach (var velocitiesChunk in velocitiesChunks)
                        {
                            var teamTags = velocitiesChunk.GetNativeArray(typeTeamTag);
                            var physicsVelocities = velocitiesChunk.GetNativeArray(typePhysicsVelocity);
                            for (var j = 0; j < teamTags.Length; j++)
                            {
                                if (teamTags[j].Id != moveCommands[i].Id) continue;
                                UnsafeUtilityEx
                                    .ArrayElementAsRef<PhysicsVelocity>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(physicsVelocities), j)
                                    .Linear += moveCommands[i].DeltaVelocity;
                            }
                        }
                    }
                }
            }
        }
    }
}
