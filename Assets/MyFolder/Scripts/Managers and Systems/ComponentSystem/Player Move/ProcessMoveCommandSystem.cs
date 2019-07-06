using System;
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
            var c3 = new NativeArray<ComponentType>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<MoveCommand>(),
                [1] = ComponentType.ReadOnly<DateTimeTicksToProcess>(),
                [2] = ComponentType.ReadWrite<DestroyableComponentData>(),
            };
            _queryMoveCommand = GetEntityQuery(c3);
            c3.Dispose();
            var c2 = new NativeArray<ComponentType>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<TeamTag>(),
                [1] = ComponentType.ReadWrite<PhysicsVelocity>(),
            };
            _query = GetEntityQuery(c2);
            c2.Dispose();
            RequireForUpdate(_query);
            RequireForUpdate(_queryMoveCommand);
        }

        protected override unsafe void OnUpdate()
        {
            var typeMoveCommand = GetArchetypeChunkComponentType<MoveCommand>(true);
            var typeDestroyable = GetArchetypeChunkComponentType<DestroyableComponentData>();
            var typeTicks = GetArchetypeChunkComponentType<DateTimeTicksToProcess>(true);
            var typeTeamTag = GetArchetypeChunkComponentType<TeamTag>(true);
            var typePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>();
            var currentTicks = DateTime.Now.Ticks;
            using (var moveCommandChunks = _queryMoveCommand.CreateArchetypeChunkArray(Allocator.TempJob))
            using (var velocitiesChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                foreach (var moveCommandChunk in moveCommandChunks)
                {
                    var moveCommands = moveCommandChunk.GetNativeArray(typeMoveCommand);
                    var destroys = moveCommandChunk.GetNativeArray(typeDestroyable);
                    var ticks = moveCommandChunk.GetNativeArray(typeTicks);
                    for (var i = 0; i < ticks.Length; i++)
                    {
                        if (ticks[i].Value < currentTicks || destroys[i].ShouldDestroy) continue;
                        destroys[i] = new DestroyableComponentData() { ShouldDestroy = true };
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
