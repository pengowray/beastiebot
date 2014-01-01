using System;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;

namespace beastie
{
	public class Bag {
		HashSet<string> words;
		HashSet<string> stems;
		HashSet<Species> species;
	}

	public class StemGroups
	{
		static public List<string> suffixes;

		//TODO: there's a one-to-one between stemGroup and group of species and group of keywords. Put them into a single class to simplify things.
		//TODO: change Species to T, so can create stems of other things too.
		private Dictionary<string, HashSet<string>> stemGroups = new Dictionary<string, HashSet<string>>(); // stem -> stemGroup
		private Dictionary<HashSet<string>, HashSet<Species>> groups = new Dictionary<HashSet<string>, HashSet<Species>>(); // stemGroup -> group of species

		private Dictionary<string, HashSet<string>> keywords = new Dictionary<string, HashSet<string>>(); // word (not stem) -> stemGroup (TODO: change to group instead of stemGroup?)
		private Dictionary<HashSet<Species>, HashSet<string>> groupwords = new Dictionary<HashSet<Species>, HashSet<string>>(); // group -> words

		//private Dictionary<string, Bag> wordIndex;
		//private Dictionary<string, Bag> stemIndex;
		//private Dictionary<Species, HashSet<Bag>> speciesIndex; //TODO


		public StemGroups()
		{
			suffixes = new List<string>(new string[] {

				//taxonomy:


				//funny endings
				"llion","llium","lli",
				"ensis", "ense", "us", 
				"ellus", "ella", "ellum",
				"ii",

				//latin declensions: // todo: be more "systematic"
				"o","ines", 
				"er","eri", "ra", "rum", "ri", "ae",
				"is", "re", "e", "ia",
				"os","us",
				"e","ae",
				"es","arum",
				"as","ae",
				"u","ua",
				"es",
				"us", "a", "um", "i", "ae", //, ""
				"x", "ges", "gis", "gum"
			} );
		}

		public void AddWord(string word, Species sp) {
			HashSet<string> myStems = null;
			HashSet<Species> myGroup = null;

			string normalizedWord = word.ToLower().Replace("-","");
			foreach (string ending in suffixes) {
				if (normalizedWord.EndsWith(ending)) {
					string stem;
					if (ending.Length > 0) {
						stem = normalizedWord.Remove(normalizedWord.Length - ending.Length);
					} else {
						stem = normalizedWord;
					}
					if (stem.Length == 0) return;

					if (! stemGroups.ContainsKey(stem)) {
						if (myStems == null) {
							myStems = new HashSet<string>();
							myGroup = new HashSet<Species>();
						}
						myStems.Add(stem);
						myGroup.Add(sp);

						stemGroups[stem] = myStems;
						groups[myStems] = myGroup;

					}  else {
						HashSet<string> existingStems = stemGroups[stem];
						HashSet<Species> existingGroup = groups[existingStems];

						if (myGroup == null) {
							myGroup = existingGroup;
							myStems = existingStems;
							myGroup.Add(sp);

						} else if (myGroup != existingGroup) {
							//merge myGroup and existing
							myGroup.UnionWith(existingGroup);
							myStems.UnionWith(existingStems);
							//myGroup.Add(sp); // already added

							//replace existing
							foreach (string stemkey in existingStems) {
								stemGroups[stemkey] = myStems;
							}
							groups.Remove(existingStems);

							foreach (string keyword in groupwords[existingGroup]) {
								keywords[keyword] = myStems;
							}
							groupwords.Remove(existingGroup);

							/*
							List<string> keysToOverwrite = new List<string>();
							foreach (string key in stems.Keys) {
								if (stems[key] == existingGroup) keysToOverwrite.Add(key);
							}
							foreach (string key in keysToOverwrite) {
								stems[key] = myGroup;
							}
							*/
						} else {
							//already added
							//myGroup.Add(sp);
						}
					}
				}

			}

			keywords[word] = myStems;
			if (myGroup != null) {
				if (!groupwords.ContainsKey(myGroup)) groupwords[myGroup] = new HashSet<string>();
				groupwords[myGroup].Add(word);
			}
			
		}

		public void PrintGroup(string word = "bulbophylli") {
			HashSet<string> stemGroup = keywords[word];
			HashSet<Species> species = groups[stemGroup];
			HashSet<string> words = groupwords[species];

			string line1 = string.Join(" - ", words);
			string line2 = string.Join(", ", species);
			Console.WriteLine(line1);
			Console.WriteLine();
			Console.WriteLine(line2);
			Console.WriteLine();

		}

		public void PrintGroups() {
			//TODO: ignore duplicates
			//TODO: sort by size
			//IComparer comp = new SizeComparer();
			//List<HashSet<Species>>sortedList = groups.Values.ToList().OrderByDescending(o=>o.Count).ToList();

			//foreach (KeyValuePair<string,int> item in groups.OrderBy(key=>key.Value))
			//foreach (KeyValuePair<string,int> item in groups.OrderBy(key=>key.Value))

			//foreach (HashSet<Species> group in groups.Values.OrderBy(groups=>groups.Count)) {
			//groups.
			
			foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in groups.OrderByDescending(pair=>pair.Value.Count)) {
			
			//foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in groups) {

				HashSet<Species> group = item.Value;
				var keywordList = groupwords[group];

				string line1 = keywordList
						.OrderByDescending(i=>i)
						.Select(x => string.Format("[[{0}]]", x))
						.JoinStrings(" "); //  · 

				Console.WriteLine(string.Format("* {0}: {1}", group.Count, line1));

				//string line2 = string.Join(", ", group);
				//Console.WriteLine(line2);
				//Console.WriteLine();
			}
		}

		public void GroupStats() {
			List<int> counts = new List<int>();
			foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in groups) {
				HashSet<Species> group = item.Value;
				counts.Add(group.Count);
				//Console.WriteLine(group.Count);
			}

			PrintStats(counts, "all");

			List<int> twoItemsOrMore = counts.FindAll(i=>i > 2);
			PrintStats(twoItemsOrMore, "2+ ");
		}

		public void PrintStats(List<int> counts, string desc = "") {

			int count = counts.Count();

			//Compute the Average
			double avg = counts.Average();
			//Perform the Sum of (value-avg)^2
			double sum = counts.Sum(d => (d - avg) * (d - avg));
			//Put it all together
			double stdev = Math.Sqrt(sum / count);
			
			Console.WriteLine(string.Format ("Stats {3}: count {0} average: {1}, stdev: {2}", count, avg, stdev, desc));

		}
	}



	//sort by largest size.
	/*
	public class SizeComparer : IComparer<HashSet<Species>>  {
		
		int IComparer.Compare( Object x, Object y )  {
			if (x is HashSet<Species> && y is HashSet<Species>) {
				return -1 * Comparer.Default.Compare((x as HashSet<Species>).Count, (y as HashSet<Species>).Count);
			} else {
				return( Comparer.Default.Compare(x, y) );
			}
		}
		
	}
*/

}

