using System;

namespace Assets.MyFolder.Scripts.Utility
{
    internal struct ArrayPool<T>
    {
        internal static T[][] Default;
    }

    public static class ArrayPool
    {
        public static object[] Get(uint length) => Get<object>(length);
        public static T[] Get<T>(uint length)
        {
            if (length == 0) return Array.Empty<T>();
            ref var arrays = ref ArrayPool<T>.Default;
            if (arrays is null)
            {
                arrays = new[]
                {
                    new T[length]
                };
                return arrays[0];
            }
            for (var i = 0; i < arrays.Length; i++)
            {
                if (arrays[i].Length == length)
                    return arrays[i];
            }
            var tmp = new T[arrays.Length + 1][];
            var answerIndex = 0;
            for (int i = 0, j = 0; i < tmp.Length;)
            {
                if (j >= arrays.Length || arrays[j].Length >= length && i == j)
                {
                    tmp[i] = new T[length];
                    answerIndex = i++;
                }
                else
                    tmp[i++] = arrays[j++];
            }
            arrays = tmp;
            return arrays[answerIndex];
        }
    }
}