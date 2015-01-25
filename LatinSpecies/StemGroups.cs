using System;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;

namespace beastie
{
	public class Bag {
		public HashSet<string> words = new HashSet<string>();
		public HashSet<string> stems = new HashSet<string>();
		public HashSet<Species> species = new HashSet<Species>();
	}

	public class StemGroups
	{
		static public List<string> suffixes;

		//TODO: there's a one-to-one between stemGroup and group of species and group of keywords. Put them into a single class to simplify things.
		//TODO: change Species to T, so can create stems of other things too.
		//private Dictionary<string, HashSet<string>> stemToStemGroup = new Dictionary<string, HashSet<string>>(); // stem -> stemGroup
		//private Dictionary<HashSet<string>, HashSet<Species>> stemGroupToSpeciesGroup = new Dictionary<HashSet<string>, HashSet<Species>>(); // stemGroup -> group of species
		//private Dictionary<string, HashSet<string>> wordToStemGroup = new Dictionary<string, HashSet<string>>(); // word (not stem) -> stemGroup (TODO: change to group instead of stemGroup?)
		//private Dictionary<HashSet<Species>, HashSet<string>> speciesGroupToWordSet = new Dictionary<HashSet<Species>, HashSet<string>>(); // group -> words

		//TODO: switch to a cleaner implementaton:
		private Dictionary<string, HashSet<Bag>> wordIndex = new Dictionary<string, HashSet<Bag>>();
		private Dictionary<string, Bag> uniWordIndex = new Dictionary<string, Bag>(); // new Dictionary<string, HashSet<Bag>>(); //TODO:
		private Dictionary<string, Bag> stemIndex = new Dictionary<string, Bag>();
		private Dictionary<Species, HashSet<Bag>> speciesIndex = new Dictionary<Species, HashSet<Bag>>();

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

		public string Stem(string word) {
			return null; //TODO
		}

		public void AddWord(string word, Species sp) {
			string normalizedWord = word.ToLower().Replace("-","");

			foreach (string ending in suffixes) {
				if (!normalizedWord.EndsWith(ending)) continue;

				string stem;
				if (ending.Length > 0) {
					stem = normalizedWord.Remove(normalizedWord.Length - ending.Length);
				} else {
					stem = normalizedWord;
				}
				if (stem == null || stem.Length == 0) continue;

				Bag bag = null;
				//wordIndex.TryGetValue(word, bag);
				stemIndex.TryGetValue(stem, out bag);
				if (bag == null) {
					bag = new Bag();
				}

				bag.species.Add(sp);
				bag.words.Add(word);
				bag.stems.Add(stem);

				stemIndex[stem] = bag;

				if (!wordIndex.ContainsKey(word)) wordIndex[word] = new HashSet<Bag>();
				//if (!wordIndex[word].Contains(bag)) 
				wordIndex[word].Add(bag);

				if (!speciesIndex.ContainsKey(sp)) speciesIndex[sp] = new HashSet<Bag>();
				speciesIndex[sp].Add(bag);
			}
		}

		public void ReduceBags() {
			//TODO
			//if a bag's words is equal or a subset of another bag's words then delete.
			/*
			foreach (Bag bag in stemIndex.Values) {
				HashSet<string> words = bag.words;
				HashSet<Bag> otherBags = wordIndex[words];
				foreach (Bag otherBag in otherBags) {
					if (bag == otherBag) continue;
					if (bag.words.Equals(otherBag.words) || bag.words.IsSubsetOf(otherBag)) {
						// kill bag.

					}
				}
			}
			*/
		}


		public Bag CombinedBagFromWord(string word) {
			HashSet<Bag> bags = wordIndex[word];
			
			Bag combinedBag = new Bag();
			foreach(Bag bag in bags) {
				combinedBag.species.UnionWith(bag.species);
				combinedBag.stems.UnionWith(bag.stems);
				combinedBag.words.UnionWith(bag.words);
			}

			return combinedBag;
		}

		public void PrintGroup(string word = "bulbophylli") {
			/*
			HashSet<string> stemGroup = wordToStemGroup[word];
			HashSet<Species> species = stemGroupToSpeciesGroup[stemGroup];
			HashSet<string> words = speciesGroupToWordSet[species];
			*/
			Bag combinedBag = CombinedBagFromWord(word);

			string line1 = string.Join(" - ", combinedBag.words);
			string line2 = string.Join(", ", combinedBag.species);
			Console.WriteLine(line1);
			Console.WriteLine();
			Console.WriteLine(line2);
			Console.WriteLine();

		}

		public void PrintGroups() {
			foreach (KeyValuePair<string, Bag> item in stemIndex.OrderByDescending(pair=>pair.Value.species.Count)) {
				string stem = item.Key;

				Bag bag = item.Value;
				HashSet<Species> species = bag.species;
				HashSet<string> words = bag.words;
				
				string line1 = words
					.OrderByDescending(i=>i)
						.Select(x => string.Format("[[{0}]]", x))
						.JoinStrings(" "); //  · 
				
				Console.WriteLine(string.Format("* {0}: {1}", species.Count, line1));

				//string line2 = string.Join(", ", species);
				//Console.WriteLine(line2);
				//Console.WriteLine();
			}
		}


		public void GroupStats() {
			List<int> counts = new List<int>();
			//foreach (KeyValuePair<HashSet<string>, HashSet<Species>> item in stemGroupToSpeciesGroup) {
			foreach (KeyValuePair<string, Bag> item in stemIndex) {
				HashSet<Species> group = item.Value.species;
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

