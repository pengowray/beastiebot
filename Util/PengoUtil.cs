using System;
using System.Collections;
using System.Collections.Generic;

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

		//public static void GetValueOrDefault<T,K>(this IDictionary<T, List<K>> dict, T key, K value) {

	}
}

