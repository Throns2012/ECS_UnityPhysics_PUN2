using Assets.MyFolder.Scripts.Basics;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup)), UpdateBefore(typeof(DestroySystem))]
    public sealed class SyncSystem : JobComponentSystem
    {
        private EntityQuery _query;
        private EntityQuery _dstQuery;
        protected override void OnCreate()
        {
            var componentTypes6 = new ComponentType[]
            {
                ComponentType.ReadOnly<PlayerMachineTag>(),
                ComponentType.ReadOnly<TeamTag>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadOnly<PhysicsVelocity>(),
                ComponentType.ReadWrite<DestroyableComponentData>()
            };
            _dstQuery = GetEntityQuery(componentTypes6);
            componentTypes6[0] = ComponentType.ReadOnly<SyncInfoTag>();
            _query = GetEntityQuery(componentTypes6);
            RequireForUpdate(_dstQuery);
            RequireForUpdate(_query);
        }

        [BurstCompile]
        struct Job : IJobChunk
        {
            public int CurrentTimestamp;
            [ReadOnly] public ArchetypeChunkComponentType<TeamTag> TypeTeamTag;
            [ReadOnly] public ArchetypeChunkComponentType<SyncInfoTag> TypeSyncInfoTag;
            public ArchetypeChunkComponentType<Translation> TypeTranslation;
            public ArchetypeChunkComponentType<Rotation> TypeRotation;
            public ArchetypeChunkComponentType<PhysicsVelocity> TypePhysicsVelocity;
            public ArchetypeChunkComponentType<DestroyableComponentData> TypeDestroyable;
            [DeallocateOnJobCompletion, ReadOnly] public NativeArray<ArchetypeChunk> SyncChunks;

            public void Execute(ArchetypeChunk chunk, int ci, int firstEntityIndex)
            {
                var shouldDestroy = new DestroyableComponentData { ShouldDestroy = true };
                var dstTranslations = chunk.GetNativeArray(TypeTranslation);
                var dstTeamTags = chunk.GetNativeArray(TypeTeamTag);
                var dstRotations = chunk.GetNativeArray(TypeRotation);
                var dstVelocities = chunk.GetNativeArray(TypePhysicsVelocity);

                const float ratio = 0f / 16f;

                for (int i = 0; i < dstTeamTags.Length; i++)
                {
                    var team = dstTeamTags[i].Id;

                    for (var chunkIndex = 0; chunkIndex < SyncChunks.Length; chunkIndex++)
                    {
                        var syncChunk = SyncChunks[chunkIndex];
                        var destroyableComponentDatas = syncChunk.GetNativeArray(TypeDestroyable);
                        for (var j = 0; j < destroyableComponentDatas.Length; j++)
                            destroyableComponentDatas[j] = shouldDestroy;
                        var syncInfoTags = syncChunk.GetNativeArray(TypeSyncInfoTag);
                        var teams = syncChunk.GetNativeArray(TypeTeamTag);
                        var translations = syncChunk.GetNativeArray(TypeTranslation);
                        var rotations = syncChunk.GetNativeArray(TypeRotation);
                        var velocities = syncChunk.GetNativeArray(TypePhysicsVelocity);
                        for (var index = 0; index < teams.Length; index++)
                        {
                            if (teams[index].Id != team) continue;
                            dstTranslations[i] = new Translation
                            {
                                Value = translations[index].Value
                                         + velocities[index].Linear
                                         * (CurrentTimestamp - syncInfoTags[index].SentServerTimestamp)
                                         * 0.001f
                            };
                            dstRotations[i] = new Rotation
                            {
                                Value = rotations[index].Value.value
                            };
                            dstVelocities[i] = velocities[index];
                            goto LOOPEND;
                        }
                    }
                LOOPEND:;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var shouldDestroy = new DestroyableComponentData { ShouldDestroy = true };
            var syncChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var typeDestroyable = GetArchetypeChunkComponentType<DestroyableComponentData>();
            for (var chunkIndex = 0; chunkIndex < syncChunks.Length; chunkIndex++)
            {
                var syncChunk = syncChunks[chunkIndex];
                var destroyableComponentDatas = syncChunk.GetNativeArray(typeDestroyable);
                for (var j = 0; j < destroyableComponentDatas.Length; j++)
                    destroyableComponentDatas[j] = shouldDestroy;
            }
            var job = new Job
            {
                CurrentTimestamp = PhotonNetwork.ServerTimestamp,
                TypeTeamTag = GetArchetypeChunkComponentType<TeamTag>(true),
                TypeSyncInfoTag = GetArchetypeChunkComponentType<SyncInfoTag>(true),
                TypeTranslation = GetArchetypeChunkComponentType<Translation>(),
                TypeRotation = GetArchetypeChunkComponentType<Rotation>(),
                TypePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>(),
                TypeDestroyable = typeDestroyable,
                SyncChunks = syncChunks,
            };
            return job.Schedule(_dstQuery, inputDeps);
        }
    }
}
