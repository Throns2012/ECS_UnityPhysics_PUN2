using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed unsafe class SerializeEverythingSystem : ComponentSystem, IInitialSerializer, IInitialDeserializer
    {
        public Entity PlayerMachinePrefabEntity;
        public Entity CurrentPointPrefabEntity;
        public Entity NextPointPrefabEntity;

        private EntityQuery _playerMachineQuery;
        private EntityQuery _pointQuery;
        private EntityQuery _pointNextQuery;

        protected override void OnCreate()
        {
            var cs = new NativeArray<ComponentType>(6, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<PlayerMachineTag>(),
                [1] = ComponentType.ReadOnly<Translation>(),
                [2] = ComponentType.ReadOnly<Rotation>(),
                [3] = ComponentType.ReadOnly<PhysicsVelocity>(),
                [4] = ComponentType.ReadOnly<TeamTag>(),
                [5] = ComponentType.ReadOnly<DestroyableComponentData>(),
            };
            _playerMachineQuery = GetEntityQuery(cs);
            cs.Dispose();
            cs = new NativeArray<ComponentType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<Translation>(),
                [1] = ComponentType.ReadOnly<PhysicsVelocity>(),
                [2] = ComponentType.ReadOnly<Point>(),
                [3] = ComponentType.ReadOnly<DestroyableComponentData>(),
            };
            _pointQuery = GetEntityQuery(cs);
            cs.Dispose();
            cs = new NativeArray<ComponentType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
            {
                [0] = ComponentType.ReadOnly<Translation>(),
                [1] = ComponentType.ReadOnly<Point>(),
                [2] = ComponentType.ReadOnly<Disabled>(),
                [3] = ComponentType.ReadOnly<DateTimeTicksToProcess>(),
            };
            _pointNextQuery = GetEntityQuery(cs);
            cs.Dispose();
        }

        protected override void OnUpdate() { }

        [BurstCompile]
        struct PlayerMachineSerializeJob : IJob
        {
            [NativeDisableUnsafePtrRestriction, NativeDisableParallelForRestriction]
            public byte* DestinationPtr;

            public long Count;

            [ReadOnly] public ArchetypeChunkComponentType<Translation> TypeTranslation;
            [ReadOnly] public ArchetypeChunkComponentType<Rotation> TypeRotation;
            [ReadOnly] public ArchetypeChunkComponentType<TeamTag> TypeTeam;
            [ReadOnly] public ArchetypeChunkComponentType<PhysicsVelocity> TypePhysicsVelocity;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;

            public void Execute()
            {
                var translationPtr = (Translation*)DestinationPtr;
                var rotationPtr = (Rotation*)(translationPtr + Count);
                var teamTagPtr = (TeamTag*)(rotationPtr + Count);
                var physicsVelocityPtr = (PhysicsVelocity*)(teamTagPtr + Count);

                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    var count = chunk.Count;
                    var translations = chunk.GetNativeArray(TypeTranslation);

                    MemCpyUtility.MemCpy(translationPtr, translations);
                    MemCpyUtility.MemCpy(rotationPtr, chunk.GetNativeArray(TypeRotation));
                    MemCpyUtility.MemCpy(teamTagPtr, chunk.GetNativeArray(TypeTeam));
                    MemCpyUtility.MemCpy(physicsVelocityPtr, chunk.GetNativeArray(TypePhysicsVelocity));

                    translationPtr += count;
                    rotationPtr += count;
                    teamTagPtr += count;
                    physicsVelocityPtr += count;
                }
            }
        }

        [BurstCompile]
        struct CurrentPointSerializeJob : IJob
        {
            [NativeDisableUnsafePtrRestriction, NativeDisableParallelForRestriction]
            public byte* DestinationPtr;

            public long Count;

            [ReadOnly] public ArchetypeChunkComponentType<Translation> TypeTranslation;
            [ReadOnly] public ArchetypeChunkComponentType<Point> TypePoint;
            [ReadOnly] public ArchetypeChunkComponentType<PhysicsVelocity> TypePhysicsVelocity;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;

            public void Execute()
            {
                var translationPtr = (Translation*)DestinationPtr;
                var pointPtr = (Point*)(translationPtr + Count);
                var linearPtr = (PhysicsVelocity*)(pointPtr + Count);

                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    var count = chunk.Count;

                    MemCpyUtility.MemCpy(translationPtr, chunk.GetNativeArray(TypeTranslation));
                    MemCpyUtility.MemCpy(pointPtr, chunk.GetNativeArray(TypePoint));
                    MemCpyUtility.MemCpy(linearPtr, chunk.GetNativeArray(TypePhysicsVelocity));

                    translationPtr += count;
                    pointPtr += count;
                    linearPtr += count;
                }
            }
        }

        [BurstCompile]
        struct NextPointSerializeJob : IJob
        {
            [NativeDisableUnsafePtrRestriction, NativeDisableParallelForRestriction]
            public byte* DestinationPtr;

            public long Count;

            [ReadOnly] public ArchetypeChunkComponentType<Translation> TypeTranslation;
            [ReadOnly] public ArchetypeChunkComponentType<Point> TypePoint;
            [ReadOnly] public ArchetypeChunkComponentType<DateTimeTicksToProcess> TypeTicks;
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;

            public void Execute()
            {
                var translationPtr = (Translation*)DestinationPtr;
                var pointPtr = (Point*)(translationPtr + Count);
                var ticksPtr = (DateTimeTicksToProcess*)(pointPtr + Count);

                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    var count = chunk.Count;
                    var translations = chunk.GetNativeArray(TypeTranslation);

                    MemCpyUtility.MemCpy(translationPtr, translations);
                    MemCpyUtility.MemCpy(pointPtr, chunk.GetNativeArray(TypePoint));
                    MemCpyUtility.MemCpy(ticksPtr, chunk.GetNativeArray(TypeTicks));

                    translationPtr += count;
                    pointPtr += count;
                    ticksPtr += count;
                }
            }
        }

        public byte[] Serialize()
        {
            var TypeTranslation = GetArchetypeChunkComponentType<Translation>(true);
            var TypeRotation = GetArchetypeChunkComponentType<Rotation>(true);
            var TypeTeam = GetArchetypeChunkComponentType<TeamTag>(true);
            var TypePoint = GetArchetypeChunkComponentType<Point>(true);
            var TypeTicks = GetArchetypeChunkComponentType<DateTimeTicksToProcess>(true);
            var TypePhysicsVelocity = GetArchetypeChunkComponentType<PhysicsVelocity>(true);

            var inputLength = sizeof(long) * 3L;
            byte* inputPtr;
            using (var playerChunks = _playerMachineQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            using (var pointChunks = _pointQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            using (var nextPointChunks = _pointNextQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var playerCount = 0L;
                for (var i = 0; i < playerChunks.Length; i++)
                {
                    playerCount += playerChunks[i].Count;
                }
                var playerByteLength = playerCount * (sizeof(Translation) + sizeof(Rotation) + sizeof(PhysicsVelocity) + sizeof(TeamTag));
                inputLength += playerByteLength;

                var pointCount = 0L;
                for (var i = 0; i < pointChunks.Length; i++)
                {
                    pointCount += pointChunks[i].Count;
                }
                var currentPointByteLength = pointCount * (sizeof(Translation) + sizeof(Point) + sizeof(PhysicsVelocity));
                inputLength += currentPointByteLength;

                var nextPointCount = 0L;
                for (var i = 0; i < nextPointChunks.Length; i++)
                {
                    nextPointCount += nextPointChunks[i].Count;
                }
                var nextPointByteLength = nextPointCount * (sizeof(Translation) + sizeof(Point) + sizeof(DateTimeTicksToProcess));
                inputLength += nextPointByteLength;

                if (inputLength > int.MaxValue)
                {
                    throw new ApplicationException();
                }
                inputPtr = (byte*)UnsafeUtility.Malloc(inputLength, 4, Allocator.TempJob);

                *(long*)inputPtr = playerCount;
                *(long*)(inputPtr + sizeof(long)) = pointCount;
                *(long*)(inputPtr + sizeof(long) * 2) = nextPointCount;

                var playerMachineFillJob = new PlayerMachineSerializeJob
                {
                    Chunks = playerChunks,
                    Count = playerCount,
                    DestinationPtr = inputPtr + sizeof(long) * 3,
                    TypePhysicsVelocity = TypePhysicsVelocity,
                    TypeTranslation = TypeTranslation,
                    TypeRotation = TypeRotation,
                    TypeTeam = TypeTeam,
                };
                var job0 = playerMachineFillJob.Schedule();
                var currentPointFillJob = new CurrentPointSerializeJob
                {
                    Chunks = pointChunks,
                    TypePhysicsVelocity = TypePhysicsVelocity,
                    TypeTranslation = TypeTranslation,
                    Count = pointCount,
                    DestinationPtr = playerMachineFillJob.DestinationPtr + playerByteLength,
                    TypePoint = TypePoint,
                };
                var job1 = currentPointFillJob.Schedule();
                var nextPointFillJob = new NextPointSerializeJob
                {
                    Chunks = nextPointChunks,
                    TypeTranslation = TypeTranslation,
                    Count = nextPointCount,
                    DestinationPtr = currentPointFillJob.DestinationPtr + currentPointByteLength,
                    TypePoint = TypePoint,
                    TypeTicks = TypeTicks,
                };
                var job2 = nextPointFillJob.Schedule();
                JobHandle.CompleteAll(ref job0, ref job1, ref job2);
            }
            var answer = Lz4Encode(inputLength, inputPtr);
            UnsafeUtility.Free(inputPtr, Allocator.TempJob);
            return answer;
        }

        private static byte[] Lz4Encode(long inputLength, byte* inputPtr)
        {
            var neededLength = Lz4Codec.MaximumOutputLength((int)inputLength);
            var outputPtr = (byte*)UnsafeUtility.Malloc(neededLength, 4, Allocator.TempJob);

            var encodedLength = Lz4Codec.Encode(inputPtr, (int)inputLength, outputPtr, neededLength);
            var answer = new byte[encodedLength + 4];

            fixed (byte* answerPtr = &answer[0])
            {
                *(int*)answerPtr = (int)inputLength;
                UnsafeUtility.MemCpy(answerPtr + 4, outputPtr, encodedLength);
            }
            UnsafeUtility.Free(outputPtr, Allocator.TempJob);
            return answer;
        }

        private static byte[] Encode(long inputLength, byte* inputPtr)
        {
            var answer = new byte[inputLength];
            fixed (byte* answerPtr = &answer[0])
            {
                UnsafeUtility.MemCpy(answerPtr, inputPtr, inputLength);
            }
            return answer;
        }


        struct DeserializeJob<T> where T : unmanaged, IComponentData
        {
            public NativeArray<Entity> Entities;
            public EntityManager Manager;
            [NativeDisableUnsafePtrRestriction] public T* Ptr;

            public void Execute()
            {
                for (var index = 0; index < Entities.Length; index++)
                    Manager.SetComponentData(Entities[index], Ptr[index]);
            }
        }

        public void Deserialize(byte[] serializedBytes)
        {
            byte* outputPtr = Lz4Decode(serializedBytes, Allocator.TempJob);
            var ptr = outputPtr;
            var playerCount = *(long*)ptr;
            ptr += 8;
            var currentPointCount = *(long*)ptr;
            ptr += 8;
            var nextPointCount = *(long*)ptr;
            ptr += 8;
            var playerTranslationPtr = (Translation*)ptr;
            var playerRotationPtr = (Rotation*)(playerTranslationPtr + playerCount);
            var playerTeamTagPtr = (TeamTag*)(playerRotationPtr + playerCount);
            var playerPhysicsVelocityPtr = (PhysicsVelocity*)(playerTeamTagPtr + playerCount);
            var currentPointTranslationPtr = (Translation*)(playerPhysicsVelocityPtr + playerCount);
            var currentPointPointPtr = (Point*)(currentPointTranslationPtr + currentPointCount);
            var currentPointPhysicsVelocityPtr = (PhysicsVelocity*)(currentPointPointPtr + currentPointCount);
            var nextPointTranslationPtr = (Translation*)(currentPointPhysicsVelocityPtr + currentPointCount);
            var nextPointPointPtr = (Point*)(nextPointTranslationPtr + nextPointCount);
            var nextPointTicksPtr = (DateTimeTicksToProcess*)(nextPointPointPtr + nextPointCount);

            using (var players = new NativeArray<Entity>((int)playerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var currentPoints = new NativeArray<Entity>((int)currentPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var nextPoints = new NativeArray<Entity>((int)nextPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                EntityManager.Instantiate(PlayerMachinePrefabEntity, players);
                EntityManager.Instantiate(CurrentPointPrefabEntity, currentPoints);
                EntityManager.Instantiate(NextPointPrefabEntity, nextPoints);

                new DeserializeJob<Translation>
                {
                    Manager = EntityManager,
                    Entities = players,
                    Ptr = playerTranslationPtr,
                }.Execute();
                new DeserializeJob<Rotation>
                {
                    Manager = EntityManager,
                    Entities = players,
                    Ptr = playerRotationPtr,
                }.Execute();
                new DeserializeJob<TeamTag>
                {
                    Manager = EntityManager,
                    Entities = players,
                    Ptr = playerTeamTagPtr,
                }.Execute();
                new DeserializeJob<PhysicsVelocity>
                {
                    Manager = EntityManager,
                    Entities = players,
                    Ptr = playerPhysicsVelocityPtr,
                }.Execute();

                new DeserializeJob<Translation>
                {
                    Manager = EntityManager,
                    Entities = currentPoints,
                    Ptr = currentPointTranslationPtr,
                }.Execute();
                new DeserializeJob<Point>
                {
                    Manager = EntityManager,
                    Entities = currentPoints,
                    Ptr = currentPointPointPtr,
                }.Execute();
                new DeserializeJob<PhysicsVelocity>
                {
                    Manager = EntityManager,
                    Entities = currentPoints,
                    Ptr = currentPointPhysicsVelocityPtr,
                }.Execute();

                new DeserializeJob<Translation>
                {
                    Manager = EntityManager,
                    Entities = nextPoints,
                    Ptr = nextPointTranslationPtr,
                }.Execute();
                new DeserializeJob<Point>
                {
                    Manager = EntityManager,
                    Entities = nextPoints,
                    Ptr = nextPointPointPtr,
                }.Execute();
                new DeserializeJob<DateTimeTicksToProcess>
                {
                    Manager = EntityManager,
                    Entities = nextPoints,
                    Ptr = nextPointTicksPtr,
                }.Execute();
            }
            UnsafeUtility.Free(outputPtr, Allocator.TempJob);
        }

        private static byte* Lz4Decode(byte[] serializedBytes, Allocator allocator)
        {
            byte* outputPtr;
            fixed (byte* inputPtr = &serializedBytes[0])
            {
                var outputLength = *(int*)inputPtr;
                outputPtr = (byte*)UnsafeUtility.Malloc(outputLength, 4, allocator);
                Lz4Codec.Decode(inputPtr + 4, serializedBytes.Length - 4, outputPtr, outputLength);
            }
            return outputPtr;
        }
    }
}