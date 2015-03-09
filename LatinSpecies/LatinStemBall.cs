using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	// keeps a bunch of species (and their counts)... usually with the same epithet stem
	public class LatinStemBall
	{
		private Dictionary<Species, long> speciesCount;
		private Dictionary<Species, bool> speciesMissing;

		//private Dictionary<Species, long> speciesWeight; // missing epithets have no weight, but still count for the per-line sorting 
		private Dictionary<string, long> similarGenus; // genus names that are similar to the epithet, and their counts
		private Dictionary<string, bool> stillUsed; // (was: similarOther) other taxa names that are similar to the epithet, but mainly are these taxa still used (vs obsolete synonyms)

		//reundant info
		private Dictionary<string, long> epithetCount;
		private Dictionary<string, long> epithetMissingCount; // same as above, but only count if it's missing.. 
		public long total = 0;

		public LatinStemBall() {
			speciesCount = new Dictionary<Species, long>();
			speciesMissing = new Dictionary<Species, bool>();
			similarGenus = new Dictionary<string, long>();
			stillUsed = new Dictionary<string, bool>();
			epithetCount = new Dictionary<string, long>();
			epithetMissingCount = new Dictionary<string, long>();
			//epithetWeight = new Dictionary<string, long>();

		}

		public void Add(Species sp, long count, bool missing = true) {
			if (speciesCount.ContainsKey(sp)) {
				speciesCount[sp] += count;
			} else {
				speciesCount[sp] = count;
			}

			speciesMissing[sp] = missing;

			if (epithetCount.ContainsKey(sp.epithet)) {
				epithetCount[sp.epithet] += count;
			} else {
				epithetCount[sp.epithet] = count;
			}

			// note this is only used for FirstDelcScore
			if (missing) {
				if (epithetMissingCount.ContainsKey(sp.epithet)) {
					epithetMissingCount[sp.epithet] += count;
				} else {
					epithetMissingCount[sp.epithet] = count;
				}
			}

			if (missing) // only give weight to the missing epithets
				total += count;
		}

		public string bestStem = null; // populated when you call found by calling FirstDeclScore()

		public long FeminineScore() {
			long ae = 0;
			bool hasOtherEndings = false;

			foreach (string key in epithetCount.Keys) {
				if (key.EndsWith("ae") || key.EndsWith("æ")) {
					ae += epithetMissingCount[key];
				} else if (key.EndsWith("i")) {
					// ignore
				} else {
					hasOtherEndings = true;
				}
			}

			if (hasOtherEndings)
				return 0;

			return ae;
		}

		public long FirstDeclScore(bool harsh=true) {
			long best = 0;

			foreach (string key in epithetCount.Keys) {
				string[] endings = new string[] { "us", "a", "um", "i", "ae", "arum", "orum" };

				long us = 0;
				long a = 0;
				long um = 0;

				long i = 0;
				long ae = 0;
				long arum = 0;
				long orum = 0;

				//if (key.EndsWith("us")) {
				foreach (string ending in endings) {
					if (!key.EndsWith(ending))
						continue;

					// stem and see if there's an -a and -um too
					string stem = key.Substring(0, key.Length - ending.Length);

					var epithets = (harsh ? epithetMissingCount : epithetCount); // don't count non-missing ones if we're being harsh
					//us = epithetMissingCount[key];
					epithets.TryGetValue(stem + "us", out us);
					epithets.TryGetValue(stem + "a", out a);
					epithets.TryGetValue(stem + "um", out um);
					epithets.TryGetValue(stem + "i", out i);
					epithets.TryGetValue(stem + "ae", out ae);
					epithets.TryGetValue(stem + "arum", out arum);
					epithets.TryGetValue(stem + "orum", out orum);

					//double score = Math.Log(us) * Math.Log(a) * Math.Log(um);
					long score = 0;
					if (harsh) {
						score = Math.Min(us, a);
						score = Math.Min(score, um);

					} else {
						score = (us + a + um) * 2
							+ i + ae + arum + orum;

					}

					if (score > best) {
						best = score;
						bestStem = stem;
					}

				}
			}

			return best;
		}

		public string Descendants() { 
			if (bestStem == null) {
				FirstDeclScore();
			}
			if (bestStem == null) {
				return string.Empty;
			}

			List<Species> speciesList = new List<Species>();
			string[] endings = new string[] { "us", "a", "um", "ae", "i", "arum", "orum" };
			foreach (Species sp in speciesCount.Keys) {
				foreach (string ending in endings) {
					if (sp.epithet == bestStem + ending) {
						speciesList.Add(sp);
						//continue; // TODO should continue outer loop
					}
				}
			}

			var ssps = speciesList.OrderBy(sp => sp.ToString());
			int smallHalf = ssps.Count() / 2;
			int bigHalf = ssps.Count() - smallHalf; // bigger half

			string top = @"{{desc-top|Translingual descendants}}";
			string mid = @"{{desc-mid}}";
			string bot = @"{{desc-bottom}}";

			string output = top + "\n"; //TODO use a string buffer
			int count = 0;
			foreach (Species sp in ssps) {
				//output += "* {{taxlink|" + sp.ToString() + "|species}}\n";
				if (speciesMissing[sp]) {
					output += "* {{spelink|" + sp.ToString() + "}}\n";
				} else {
					output += "* ''[[" + sp.ToString() + "]]''\n";
				}

				if (count == smallHalf) {
					output += mid + "\n";
				}

				count++;
			}
			output += bot; // + "\n";

			return output;
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

		public string PrettyGenusListNoNumbers() {
			return PrettyGenusList(false);
		}

		public string PrettyGenusList(bool withNumbers = true) {
			// {{taxon|genus|family|Boidae|the [[boa]]s}}
			//string.Format("#** {0} # {{taxon|genus|family|{1}|[[{2}]]}}", genus, family, genusCommonName);

			var gens = 
				from entry in similarGenus
				orderby entry.Value descending
				//select ("&lt; {{l|mul|" + entry.Key + "}}" );
//				select ("{{l|mul|" + entry.Key + "}}" + PrettyValue(entry.Value) );
				select (string.Format("[[{0}#Translingual|{0}]]{1}{2}", entry.Key, DoubleDagger(entry.Key), PrettyValue(entry.Value, withNumbers)));

			//string genusString = string.Join(", ", gens);
			string genusString = string.Join(", ", gens);

			return genusString;
		}

		// don't print large numbers
		private string PrettyValue(long val, bool withNumbers = true) {
			int max = 100; 

			if (!withNumbers)
				return "";

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

