using System;
using Assets.MyFolder.Scripts.Basics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public class SerializeEverythingSystem : ComponentSystem, IInitialSerializer
    {
        private EntityQuery _playerMachineQuery;
        private EntityQuery _pointQuery;
        private EntityQuery _pointNextQuery;

        protected override void OnCreate()
        {
            Enabled = false;
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
        unsafe struct PlayerMachineFillJob : IJob
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
        unsafe struct CurrentPointFillJob : IJob
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
                var linearPtr = (float3*)(pointPtr + Count);

                for (var i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];
                    var count = chunk.Count;

                    MemCpyUtility.MemCpy(translationPtr, chunk.GetNativeArray(TypeTranslation));
                    MemCpyUtility.MemCpy(pointPtr, chunk.GetNativeArray(TypePoint));
                    MemCpyUtility.MemCpyStride(linearPtr, chunk.GetNativeArray(TypePhysicsVelocity));

                    translationPtr += count;
                    pointPtr += count;
                    linearPtr += count;
                }
            }
        }

        [BurstCompile]
        unsafe struct NextPointFillJob : IJob
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

        public unsafe byte[] Serialize()
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
                var currentPointByteLength = pointCount * (sizeof(Translation) + sizeof(Point) + sizeof(float3) /* Linear of PhysicsVelocity */);
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

                var playerMachineFillJob = new PlayerMachineFillJob()
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
                var currentPointFillJob = new CurrentPointFillJob()
                {
                    Chunks = pointChunks,
                    TypePhysicsVelocity = TypePhysicsVelocity,
                    TypeTranslation = TypeTranslation,
                    Count = pointCount,
                    DestinationPtr = playerMachineFillJob.DestinationPtr + playerByteLength,
                    TypePoint = TypePoint,
                };
                var job1 = currentPointFillJob.Schedule();
                var nextPointFillJob = new NextPointFillJob()
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
            var neededLength = Lz4Codec.MaximumOutputLength((int)inputLength);
            var outputPtr = (byte*)UnsafeUtility.Malloc(neededLength, 4, Allocator.TempJob);

            var encodedLength = Lz4Codec.Encode(inputPtr, (int)inputLength, outputPtr, neededLength);
            var answer = new byte[encodedLength + 4];

            fixed (byte* answerPtr = &answer[0])
            {
                *(int*)answerPtr = (int)inputLength;
                UnsafeUtility.MemCpy(answerPtr, outputPtr, encodedLength);
            }
            UnsafeUtility.Free(inputPtr, Allocator.TempJob);
            UnsafeUtility.Free(outputPtr, Allocator.TempJob);
            return answer;
        }
    }
}