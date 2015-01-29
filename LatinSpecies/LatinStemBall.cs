using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	// keeps a bunch of species (and their counts)... usually with the same epithet stem
	public class LatinStemBall
	{
		private Dictionary<Species, long> speciesCount;

		//reundant info
		public long total = 0;

		public LatinStemBall() {
			speciesCount = new Dictionary<Species, long>();
		}

		public void Add(Species sp, long count) {
			if (speciesCount.ContainsKey(sp)) {
				speciesCount[sp] += count;
			} else {
				speciesCount[sp] = count;
			}
			total += count;
		}

		public string PrettyPrint() {
			HashSet<string> epithets = new HashSet<string>();

			// all epithets... order alphabetically? or by count? or by most likely lemma?
			foreach (var sp in speciesCount.Keys) {
				epithets.Add(sp.epithet);
			}

			var eps = from entry in epithets orderby entry select "[[" + entry + "]]"; //ascending 
			string epithetsString = string.Join(", ", eps);

			return epithetsString;
		}

		public string PrettyExamples(int max = 5) {
			// top 5 examples
			var examples = (from entry in speciesCount orderby entry.Value descending
				select "''[[" + entry.Key + "]]''").Take(max);
			string examplesString = string.Join(", ", examples);

			return examplesString;

		}

		public static void Test() {
			var testers = new string[] {
				"gracilis","gracile","graciles","gracilia",
				"bicolor", "bicolores", "bicolōria", "bicolōris", "bicolōrium",
				"minor", "minus", "minōrēs", "minōra", // minus (adj nominative sing. neuter) it gets wrong ("min" instead of "minor".)
				"intermedius", "intermedia", "intermedium", 
				"intermediī", "intermediae", "intermedia", // plurals (not so important)
				"longipēs", "longipēs", 
				"longipedēs", "longipedia", // plurals (not so important)
				"longipedis", "longipedis", // genitive	
				"flāvus", "flāva", "flāvum",
				"japonicus", "japonica", "japonicum",
				"darwinii", "darwini",
				"longicornis", "longicorne",
				"californicus", "californica", "californicum",
				"hystrīx", "hystrīcēs", "hystrīcis", // noun, noun-pl, adj-genative
			};

			foreach (var test in testers) {
				var stemmedTest = LatinStemmer.stemAsNoun(test);
				Console.WriteLine("{0} : {1}", stemmedTest, test);
			}
		}


	}
}

