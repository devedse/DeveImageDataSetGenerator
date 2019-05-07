using System;
using System.Collections.Generic;
using System.Linq;

namespace DeveImageDataSetGenerator.Helpers
{
    public static class ListRandomizer
    {
        public static IList<T> Shuffle<T>(IEnumerable<T> list, int seed)
        {
            var copyOfList = list.ToList();

            var rng = new Random(seed);
            int n = copyOfList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = copyOfList[k];
                copyOfList[k] = copyOfList[n];
                copyOfList[n] = value;
            }

            return copyOfList;
        }
    }
}
