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
		private Dictionary<string, HashSet<string>> stemToStemGroup = new Dictionary<string, HashSet<string>>(); // stem -> stemGroup
		private Dictionary<HashSet<string>, HashSet<Species>> stemGroupToSpeciesGroup = new Dictionary<HashSet<string>, HashSet<Species>>(); // stemGroup -> group of species

		private Dictionary<string, HashSet<string>> wordToStemGroup = new Dictionary<string, HashSet<string>>(); // word (not stem) -> stemGroup (TODO: change to group instead of stemGroup?)
		private Dictionary<HashSet<Species>, HashSet<string>> speciesGroupToWordSet = new Dictionary<HashSet<Species>, HashSet<string>>(); // group -> words

		//TODO: switch to a cleaner implementaton:
		//private Dictionary<string, Bag> wordIndex;
		//private Dictionary<string, Bag> stemIndex;
		//private Dictionary<Species, HashSet<Bag>> speciesIndex; //TODO later



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
				"x", "ges", "gis", "gum", ""
			} );
		}

		public void AddWord(string word, Species sp) {
			HashSet<string> myStemGroup = null;
			HashSet<Species> mySpeciesGroup = null;

			// stemToStemGroup = new Dictionary<string, HashSet<string>>(); // stem -> stemGroup
			// stemGroupToSpeciesGroup = new Dictionary<HashSet<string>, HashSet<Species>>(); // stemGroup -> group of species
			// wordToStemGroup = new Dictionary<string, HashSet<string>>(); // word (not stem) -> stemGroup (TODO: change to group instead of stemGroup?)
			// speciesGroupToWordSet = new Dictionary<HashSet<Species>, HashSet<string>>(); // group -> words

			string normalizedWord = word.ToLower().Replace("-","");

			foreach (string ending in suffixes) {
				if (normalizedWord.EndsWith(ending)) {
					string stem;
					if (ending.Length > 0) {
						stem = normalizedWord.Remove(normalizedWord.Length - ending.Length);
					} else {
						stem = normalizedWord;
					}
					if (stem == null || stem.Length == 0) continue;

					if (! stemToStemGroup.ContainsKey(stem)) {
						if (myStemGroup == null) {
							myStemGroup = new HashSet<string>();
							mySpeciesGroup = new HashSet<Species>();
						}
						myStemGroup.Add(stem);
						mySpeciesGroup.Add(sp);

						stemToStemGroup[stem] = myStemGroup;
						stemGroupToSpeciesGroup[myStemGroup] = mySpeciesGroup;

					}  else {

						HashSet<string> existingStemGroup = stemToStemGroup[stem];
						HashSet<Species> existingSpeciesGroup = stemGroupToSpeciesGroup[existingStemGroup];

						if (myStemGroup == null) {
							mySpeciesGroup = existingSpeciesGroup;
							myStemGroup = existingStemGroup;
							mySpeciesGroup.Add(sp);
							myStemGroup.Add(stem); // redundant
							stemGroupToSpeciesGroup[myStemGroup] = mySpeciesGroup;
							wordToStemGroup[word] = myStemGroup; // redundant (done later)

						} else if (! Object.ReferenceEquals(mySpeciesGroup, existingSpeciesGroup)) {
							//merge myGroup and existing
							mySpeciesGroup.UnionWith(existingSpeciesGroup);
							myStemGroup.UnionWith(existingStemGroup);

							myStemGroup.Add(stem); // redundant
							mySpeciesGroup.Add(sp); // redundant?

							//replace existing
							foreach (string stemkey in existingStemGroup) {
								stemToStemGroup[stemkey] = myStemGroup;
							}
							stemGroupToSpeciesGroup.Remove(existingStemGroup);
							stemGroupToSpeciesGroup[myStemGroup] = mySpeciesGroup;
							stemToStemGroup[stem] = myStemGroup;

							foreach (string othersWord in speciesGroupToWordSet[existingSpeciesGroup]) {
								wordToStemGroup[othersWord] = myStemGroup;
							}
							wordToStemGroup[word] = myStemGroup; // redundant (done later)

							//speciesGroupToWordSet.Remove(existingGroup);

						} else {
							//already got the right stems / groups.. but is it already added?
							mySpeciesGroup.Add(sp); // probably not needed
							myStemGroup.Add(stem); // also probably redundant
							stemGroupToSpeciesGroup[myStemGroup] = mySpeciesGroup;
							stemToStemGroup[stem] = myStemGroup; // very redundant
							wordToStemGroup[word] = myStemGroup; // redundant (done later)

						}
					}
				}

			}

			if (mySpeciesGroup != null && myStemGroup != null) {
				//wordToStemGroup[word] = myStemGroup;
				if (!speciesGroupToWordSet.ContainsKey(mySpeciesGroup)) speciesGroupToWordSet[mySpeciesGroup] = new HashSet<string>();
				speciesGroupToWordSet[mySpeciesGroup].Add(word);
			}
			
		}

		public void PrintGroup(string word = "bulbophylli") {
			HashSet<string> stemGroup = wordToStemGroup[word];
			HashSet<Species> species = stemGroupToSpeciesGroup[stemGroup];
			HashSet<string> words = speciesGroupToWordSet[species];

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
			
			foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in stemGroupToSpeciesGroup.OrderByDescending(pair=>pair.Value.Count)) {
			
			//foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in groups) {

				HashSet<Species> group = item.Value;
				var keywordList = speciesGroupToWordSet[group];

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
			foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in stemGroupToSpeciesGroup) {
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

