using Assets.MyFolder.Scripts.Basics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Assets.MyFolder.Scripts.Managers_and_Systems
{
    public sealed class InitialDeserializeSystem : ComponentSystem, IInitialDeserializer
    {
        public Entity PlayerMachinePrefabEntity;
        public Entity CurrentPointPrefabEntity;
        public Entity NextPointPrefabEntity;
        protected override void OnUpdate()
        { }

        public unsafe void Deserialize(byte[] serializedBytes)
        {
            int outputLength;
            byte* outputPtr;
            fixed (byte* inputPtr = &serializedBytes[0])
            {
                outputLength = *(int*)inputPtr;
                outputPtr = (byte*)UnsafeUtility.Malloc(outputLength, 4, Allocator.TempJob);
                Lz4Codec.Decode(inputPtr + 4, serializedBytes.Length - 4, outputPtr, outputLength);
            }
            var ptr = outputPtr;
            var playerCount = *(long*)ptr;
            ptr += 8;
            var currentPointCount = *(long*)ptr;
            ptr += 8;
            var nextPointCount = *(long*)ptr;

            using (var players = new NativeArray<Entity>((int)playerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var currentPoints = new NativeArray<Entity>((int)currentPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var nextPoints = new NativeArray<Entity>((int)nextPointCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {

            }
        }
    }
}
