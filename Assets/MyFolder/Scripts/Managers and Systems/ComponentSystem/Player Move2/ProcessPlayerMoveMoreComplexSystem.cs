using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public class ProcessPlayerMoveMoreComplexSystem : ComponentSystem
    {
        private EntityQuery _query;
        private EntityQuery _queryMoveCommand;

        protected override void OnCreate()
        {
            var c4 = new NativeArray<ComponentType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<MoveCommandMoreComplex>(),
                [1] = ComponentType.ReadOnly<DateTimeTicksToProcess>(),
                [2] = ComponentType.ReadWrite<DestroyableComponentData>(),
                [3] = ComponentType.ReadWrite<TeamTag>(),
            };
            _queryMoveCommand = GetEntityQuery(c4);
            c4.Dispose();
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

        protected override void OnUpdate()
        {
            var typeMoveCommand = GetArchetypeChunkComponentType<MoveCommandMoreComplex>(true);
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
                    var commandTeamTags = moveCommandChunk.GetNativeArray(typeTeamTag);
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
                                if (teamTags[j].Id != commandTeamTags[i].Id) continue;
                                physicsVelocities[j] = moveCommands[i].Value;
                            }
                        }
                    }
                }
            }
        }
    }
}