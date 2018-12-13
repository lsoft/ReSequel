using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Main.Helper
{
    public static class ListHelper
    {
        public static bool NotIn<T>(
            this T v,
            IEnumerable<T> array
            )
        {
            return
                !array.Contains(v);
        }

        public static bool In<T>(
            this T v,
            IEnumerable<T> array
            )
        {
            return
                array.Contains(v);
        }

        public static bool NotIn<T>(
            this T v,
            params T[] array
            )
        {
            return
                !array.Contains(v);
        }

        public static bool In<T>(
            this T v,
            params T[] array
            )
        {
            return
                array.Contains(v);
        }

        public static bool NotIn<T>(
            this T v,
            IEqualityComparer<T> comparer,
            params T[] array
            )
        {
            return
                !array.Contains(v, comparer);
        }

        public static bool In<T>(
            this T v,
            IEqualityComparer<T> comparer,
            params T[] array
            )
        {
            return
                array.Contains(v, comparer);
        }

        public static IEnumerable<List<T>> Split<T>(
            this IEnumerator<T> list,
            int splitCount
            )
        {
            if (splitCount <= 0)
            {
                throw new ArgumentException("splitCount <= 0");
            }

            var nextList = new List<T>();

            while(list.MoveNext())
            {
                var item = list.Current;

                nextList.Add(item);

                if (nextList.Count == splitCount)
                {
                    yield return nextList;

                    nextList = new List<T>();
                }
            }

            //if (list.Count % splitCount != 0)
            if (nextList.Count > 0)
            {
                yield return nextList;
            }
        }

        public static IEnumerable<List<T>> Split<T>(
            this IEnumerable<T> list,
            int splitCount
            )
        {
            if (splitCount <= 0)
            {
                throw new ArgumentException("splitCount <= 0");
            }

            var nextList = new List<T>();

            foreach(var item in list)
            {
                nextList.Add(item);

                if(nextList.Count == splitCount)
                {
                    yield return nextList;

                    nextList = new List<T>();
                }
            }

            //if (list.Count % splitCount != 0)
            if(nextList.Count > 0)
            {
                yield return nextList;
            }
        }

    }

}
