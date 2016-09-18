using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	public static class PengoUtil
	{
		// See also: MultiValueDictionary, https://www.nuget.org/packages/Microsoft.Experimental.Collections
		//public static void AddToList<T,K>(this IDictionary<T, IList<K>> dict, T key, K value) {
		public static void AddToList<T,K>(this IDictionary<T, List<K>> dict, T key, K value) {
			List<K> list = null; // IList<K> list = null;
			if (dict.TryGetValue(key, out list)) {
				list.Add(value);
			} else {
				list = new List<K>();
				list.Add(value);
				dict[key] = list;
			}
		}

		public static void AddCount<T>(this Dictionary<T, int> dict, T key, int value) {
			int currentValue = 0;
			if (dict.TryGetValue(key, out currentValue)) {
				dict[key] = currentValue + value;
			} else {
				dict[key] = value;
			}
		}

		public static void AddCount<T>(this Dictionary<T, long> dict, T key, long value) {
			long currentValue = 0;
			if (dict.TryGetValue(key, out currentValue)) {
				dict[key] = currentValue + value;
			} else {
				dict[key] = value;
			}
		}

        // via http://stackoverflow.com/a/33223183/443019 (as "GetValue")
        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV)) {
            TV value;
            return dict.TryGetValue(key, out value) ? value : default(TV);
        }

        //public static void GetValueOrDefault<T,K>(this IDictionary<T, List<K>> dict, T key, K value) {


        //https://stackoverflow.com/questions/273313/randomize-a-listt
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


/*
Allows you to do this on any kind of list:

var numbers = new List<Tuple<int, string>>
{
    { 1, "one" },
    { 2, "two" },
    { 3, "three" },
    { 4, "four" },
    { 5, "five" },
};

http://stackoverflow.com/a/27455822
*/
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list,
        T1 item1, T2 item2) {
            list.Add(Tuple.Create(item1, item2));
        }

        public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list,
                T1 item1, T2 item2, T3 item3) {
            list.Add(Tuple.Create(item1, item2, item3));
        } //TODO: etc


        // warning: may give the same result if called multiple times in quick succession
        // http://stackoverflow.com/questions/3173718/how-to-get-a-random-object-using-linq
        public static T Random<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                //throw new ArgumentNullException(nameof(enumerable));
                throw new ArgumentNullException();
            }

            // note: creating a Random instance each call may not be correct for you,
            // consider a thread-safe static instance
            var r = new Random();
            var list = enumerable as IList<T> ?? enumerable.ToList();
            return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
        }

    }



}

