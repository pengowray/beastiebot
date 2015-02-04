using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	// keeps a bunch of species (and their counts)... usually with the same epithet stem
	public class LatinStemBall
	{
		private Dictionary<Species, long> speciesCount;
		//private Dictionary<Species, long> speciesWeight; // missing epithets have no weight, but still count for the per-line sorting 
		private Dictionary<string, long> similarGenus; // genus names that are similar to the epithet, and their counts
		private Dictionary<string, bool> stillUsed; // (was: similarOther) other taxa names that are similar to the epithet, but mainly are these taxa still used (vs obsolete synonyms)

		//reundant info
		private Dictionary<string, long> epithetCount;
		public long total = 0;

		public LatinStemBall() {
			speciesCount = new Dictionary<Species, long>();
			similarGenus = new Dictionary<string, long>();
			stillUsed = new Dictionary<string, bool>();
			epithetCount = new Dictionary<string, long>();
			//epithetWeight = new Dictionary<string, long>();
		}

		public void Add(Species sp, long count, bool missing = true) {
			if (speciesCount.ContainsKey(sp)) {
				speciesCount[sp] += count;
			} else {
				speciesCount[sp] = count;
			}

			if (epithetCount.ContainsKey(sp.epithet)) {
				epithetCount[sp.epithet] += count;
			} else {
				epithetCount[sp.epithet] = count;
			}

			if (missing) // only give weight to the missing epithets
				total += count;

		}

		public void AddGenus(string genus, long count) {
			if (similarGenus.ContainsKey(genus)) {
				similarGenus[genus] += count;
			} else {
				similarGenus[genus] = count;
			}
		}

		public void StillUsed(string sci_name) {
			stillUsed[sci_name] = true;
		}
			
		public string PrettyPrint() {
			//HashSet<string> epithets = new HashSet<string>();
			//Dictionary<string, long> epithets = new Dictionary<string, long>();

			// all epithets... order alphabetically? or by count? or by most likely lemma?
			//foreach (var sp in speciesCount.Keys) {
			//	epithets.Add(sp.epithet);
			//}

				//.Select(sp => sp.Key
			//var eps = from entry in epithetCount orderby entry.Value select "[[" + entry + "]]"; //ascending 
			//var eps = from entry in epithets orderby entry.Value select ("{{l|la|" + entry + "}}") descending ;
			var eps = 
				from entry in epithetCount 
				orderby entry.Value descending
				//select ("{{l|la|" + entry.Key + "}}" + PrettyValue(entry.Value) );
				select (string.Format("[[{0}#Latin|{0}]]{1}{2}", entry.Key, DoubleDagger(entry.Key), PrettyValue(entry.Value)));

			string epithetsString = string.Join(", ", eps);

			return epithetsString;
		}

		public string DoubleDagger(string term) {
			if (stillUsed.ContainsKey(term))
				return "";

			return "‡";
		}

		public string DoubleDagger(Species binomial) {
			var details = new SpeciesDetails(binomial);
			details.Load(); // TODO: error handling
			if (details.status == Status.accepted || details.status == Status.provisionally_accepted_name) {
				return "";
			}

			return "‡";
		}

		public string PrettyExamples(int max = 5) {
			// top 5 examples
			var examples = (from entry in speciesCount orderby entry.Value descending
				//select "''[[" + entry.Key + "]]''").Take(max);
				select (string.Format("''[[{0}]]''{1}{2}", entry.Key, DoubleDagger(entry.Key), PrettyValue(entry.Value))))
				.Take(max);
			string examplesString = string.Join(", ", examples);

			return examplesString;
		}

		public string PrettyGenusList() {
			// {{taxon|genus|family|Boidae|the [[boa]]s}}
			//string.Format("#** {0} # {{taxon|genus|family|{1}|[[{2}]]}}", genus, family, genusCommonName);

			var gens = 
				from entry in similarGenus
				orderby entry.Value descending
				//select ("&lt; {{l|mul|" + entry.Key + "}}" );
//				select ("{{l|mul|" + entry.Key + "}}" + PrettyValue(entry.Value) );
				select (string.Format("[[{0}#Translingual|{0}]]{1}{2}", entry.Key, DoubleDagger(entry.Key), PrettyValue(entry.Value)));

			//string genusString = string.Join(", ", gens);
			string genusString = string.Join(", ", gens);

			return genusString;
		}

		// don't print large numbers
		private string PrettyValue(long val) {
			int max = 100; 
			if (val >= max)
				return "";

			return string.Format(" ({0})", val);
		}

		public static void Test() {
			var testers = new string[] {
				// problems
				//cereuis : cerevisiae
				//cereuis : cerevisia
				//ceruis  : cervisia
				"bōs", "bovēs", // b <= bōs.. bou <= bovēs
				"cerevisiae", "cerevisia", "cervisia",
				"minor", "minus", "minōrēs", "minōra", // minus (adj nominative sing. neuter) it gets wrong ("min" instead of "minor".)
				"īlex", "īlicis", "īlicēs", // īlicēs (plural), īlicis (genitive singular)
				"aspera", "asperum", "asper", // broken by er -> r rule
				"asperī", "asperae", // less important
				"paluster","palustris", "palustre",
				// fixed
				"melanogaster", "melanogastra", "melanogastrum", // melanogaster : melanogaster.  melanogastr : melanogastra.. er -> r ?
				"niger", "nigra", "nigri", "nigrum", // change er -> r ?
				"ruber","rubra", "rubrum", // er -> r
				"flāvus", "flāva", "flāvum", // was removing uum (vum)
				"subflavus", "subflava", "subflavum",

				//ok or good enough
				//"arborescens", "arborēscentis", // doesn't matter.
				"novaehollandiae","novae-hollandiae",
				"cerevisiae", "cerevisia", 
				"gracilis","gracile","graciles","gracilia",
				/*
				"bicolor", "bicolores", "bicolōria", "bicolōris", "bicolōrium",
				"intermedius", "intermedia", "intermedium", 
				"intermediī", "intermediae", "intermedia", // plurals (not so important)
				"longipēs", "longipēs", 
				"longipedēs", "longipedia", // plurals (not so important)
				"longipedis", "longipedis", // genitive	
				"japonicus", "japonica", "japonicum",
				"darwinii", "darwini",
				*/
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

