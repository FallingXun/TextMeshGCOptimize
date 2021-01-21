using System.Collections.Generic;


namespace TMPro
{
    internal static class TMP_ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly TMP_ObjectPool<List<T>> s_ListPool = new TMP_ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    internal static class TMP_ArrayPool<T>
    {
        private static readonly TMP_ArrayObjectPool<T> s_ArrayPool = new TMP_ArrayObjectPool<T>(null, arr => System.Array.Clear(arr, 0, arr.Length));

        public static T[] Get(int count)
        {
            return s_ArrayPool.Get(count);
        }

        public static void Release(T[] toRelease)
        {
            if (toRelease == null)
            {
                return;
            }
            s_ArrayPool.Release(toRelease.Length, toRelease);
        }
    }
}