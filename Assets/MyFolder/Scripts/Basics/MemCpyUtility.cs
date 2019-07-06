using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Assets.MyFolder.Scripts.Basics
{
    public static unsafe class MemCpyUtility
    {
        public static void MemCpy<T>(T* destination, NativeArray<T> source)
            where T : unmanaged
        {
            UnsafeUtility.MemCpy(destination, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(source), source.Length * sizeof(T));
        }

        public static void MemCpyStride<TDestination, TSource>(TDestination* destination, NativeArray<TSource> source)
            where TDestination : unmanaged
            where TSource : unmanaged
        {
            UnsafeUtility.MemCpyStride(destination, sizeof(TDestination), NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(source), sizeof(TSource), sizeof(TDestination), source.Length);
        }
    }
}