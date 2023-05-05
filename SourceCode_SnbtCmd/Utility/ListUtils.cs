using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryashtarUtils.Utility
{
    public static class ListUtils
    {
        // shuffle a list in-place
        public static void Shuffle<T>(IList<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool ExactlyOne<T>(this IEnumerable<T> items)
        {
            return items.Any() && !items.Skip(1).Any();
        }

        public static bool CountGreaterThan<T>(this IEnumerable<T> items, int count)
        {
            return items.Skip(count).Any();
        }

        // Enumerable.Range() but for longs
        public static IEnumerable<long> CreateRange(long start, long count)
        {
            var limit = start + count;
            while (start < limit)
            {
                yield return start;
                start++;
            }
        }

        public static Dictionary<T, U> Copy<T, U>(this IReadOnlyDictionary<T, U> dict)
        {
            return dict.ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
