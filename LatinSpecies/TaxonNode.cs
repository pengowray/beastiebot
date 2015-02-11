using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	public class TaxonNode
	{
		//const
		static string[] majorRanks = new string[] { "kingdom","phylum","class","order","family","genus","species" };

		/*
		 *
		 *
		 *
TODO: 
Gecarcinucidae (many redirects to genus, not monotypic). ALso: Parastacidae, and Isopoda
e.g. Ceylonthelphusa sanguinea, Thermosphaeroma cavicauda
How to handle?: 
Dexteria floridana => Dexteria (monotypic) 
Haplochromis sp. 'parvidens-like'
Lipochromis sp. nov. 'small obesoid'

Cycloramphidae wikilink Cycloramphinae // different spelling redirect (not common name)
Anura wikilink Anura (frog)  // disambig link

// (done) Holoaden bradei out of place (unsorted) (maybe an illusion)

// (done) "Not assigned"
// (done) "Cardinal (bird) species" => Cardinal species" 
// (done) too much space after Morogoro pretty grasshopper (Thericleidae), before Phasmatodea

TODO:
// (done) Huso huso => Beluga (sturgeon)
// (done) Huso dauricus => Kaluga (fish)

Blurbs under headings:
Of the 100 blah species which have been assessed by the IUCN, 55 are threatened with extinction. The 12 critically endangered of these are listed. 2 blah are listed as "data deficient". 
All the [assessed/threatened/crit] are endemic to Antarctica [and/or] China.
44 species of the 230 which have been assessed are critically endangered. An additional 12 species are classified as "Data Deficient".
(important) There are 12 critically endangered [category] ([commoon name]) species, and 2 critically endangered subspecies: blah and blah.
(important) 3 stocks/populations have been assessed as critically endangered:

Pygmy sunfish species
Some researchers believe they are related to sticklebacks and pipefishes (order Syngnathiformes) rather than Perciformes.

		static string[] animaliaBreakdown = new string[] { 
			"Mollusca", // phylum
			"fish: Actinopterygii + Chondrichthyes + Myxini + Cephalaspidomorphi + Sarcopterygii" // classes within Animalia (kingdom),Chordata (phylum)
			"Insecta", // class within Arthropoda (phylum)
			"Arachnida", // // class within Arthropoda,
			"Arthropoda -Insecta -Arachnida", // link to them // maxillopoda, malacostraca, branchiopoda, diplopoda, chilopoda, ostracoda
			"invertibrate: (+Insecta) (+Arachnida) +Cnidaria +Annelida +Onychophora +Nemertina " // worms and jellies, link to others
			"crustaceans: +Branchiopoda +Ostracoda +Malacostraca" // classes not found in iucn CR: +Remipedia +Cephalocarida

			// - Mammalia (class) - Reptilia (class) - Aves (class) - Amphibia (class)
		}
		
		{"Actinopterygii": "ray-finned fishes", // pl
			"Chondrichthyes": "cartilaginous fish", "includes sharks, rays, chimaeras", // note: chimaeras may be moved
			// "Myxini": "Hagfish" // name of wikipedia page already
			"Cephalaspidomorphi", "lamprey" // lampreys moved to Petromyzontiformes (order. the rest of the class is long extinct fossils) // //"Hyperoartia"
			"Sarcopterygii", "lobe-finned fish", "includes lungfish, coelacanths. Tetrapods are excluded here."
			"Cnidaria",, "includes sea anemones, corals, and sea jellies"
			"Annelida", "annelid (segmented worm)", "includes ragworms, earthworms and leeches."
			"Onychophora", "velvet worm"
			"Nemertina", "ribbon worm" // = Nemertea
			"Arachnida", "arachnid", "including spiders, scorpions, harvestmen, ticks, mites, and solifuges"

"Caprimulgiformes" "including the potoos, nightjars, and eared-nightjars"
		}
		*/

		//tracheophyta - Vascular plants, "also known as tracheophytes or higher plants"

		public TaxonDisplayRules rules;

		public string rank;
		public string name;

		string enwikiArticle;

		TaxonNode parent;
		List<TaxonNode> children = new List<TaxonNode>();

		List<string> bitris = new List<string>(); // species and lower level

		public bool isMajorRank() {
			return (majorRanks.Contains(rank));
		}

		public TaxonNode() {
		}

		public void Add(TaxonDetails details) {
			if (rank == "top") {
				TaxonNode current = this;
				foreach (string drank in details.ranks) {
					string dname = details.rankName[drank];

					if (string.IsNullOrWhiteSpace(dname))
						continue;

					TaxonNode tn = current.FindChild(drank, dname);
					if (tn == null) {
						tn = new TaxonNode();
						tn.rules = rules;
						tn.rank = drank;
						tn.name = dname;
						tn.parent = current;
						current.children.Add(tn);
					}
					current = tn;
				}

				if (current.rank != "top") {
					current.AddSpeciesChild(details);
				}
			} else {
				// then what?
			}


		}

		public void AddSpeciesChild(TaxonDetails details) {
			bitris.Add(details.FullSpeciesName());
		}

		string Altname() {
			return Altname(name);
		}

		static string Altname(string term) {
			//var page = BeastieBot.Instance().GetLoadedPage(term, true);
			//var page = BeastieBot.Instance().GetPage(term, true);
			string nameInWiki = BeastieBot.Instance().PageNameInWiki(term);
			if (!string.IsNullOrWhiteSpace(nameInWiki)) {
				return term + "|" + nameInWiki; // TODO: lots.. hide (genus) bit, and better search
			}
			return term;
		}

		public void PrettyPrint(int depth = 0) {
			
			string tabs = "";
			if (depth > 0) { // no tabs for kingdom
				tabs = new string('=', depth-1);
			}

			string wikiedName = "[[" + name + "]]";
			//string altname = Altname();
			//string altname = Altname(name);
			if (name == "Not assigned" || name == "ZZZZZ Not assigned") {
				wikiedName = "Not assigned";
			} else {
				string nameInWiki = null;
				if (rules != null && rules.taxonCommonName.ContainsKey(name)) {
					nameInWiki = rules.taxonCommonName[name].UpperCaseFirstChar();
				} else {
					nameInWiki = BeastieBot.Instance().PageNameInWiki(name);

					if (nameInWiki != null) {
						// ignore redirects to another family (-idae = animal, -aceae = plant/fungi/algae) e.g.
						// e.g. on Wikipedia Limnodynastidae redirects to Myobatrachidae, where it is called Limnodynastinae, and is a subfamily. Keep link just to Limnodynastidae.
						if (nameInWiki.EndsWith("idae") || nameInWiki.EndsWith("aceae")) {
							nameInWiki = null;
							//TODO: warn user
						}
					}
				}

				if (!string.IsNullOrEmpty(nameInWiki)) {

					// fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
					nameInWiki.Replace("  ", " "); 

					if (nameInWiki.Contains(" (")) {
						// remove " (insect)" from "Cricket (insect)"
						nameInWiki = nameInWiki.Substring(0, nameInWiki.IndexOf(" ("));
					}
					if (nameInWiki != name) {
						if (nameInWiki.Contains("species") || nameInWiki.Contains("family")) {
							wikiedName = string.Format("[[{0}|{1}]]", name, nameInWiki);
						} else {
							wikiedName = string.Format("[[{0}|{1}]] species", name, nameInWiki);
						}
					}
				}
			}

			//string line = string.Format("{0}[[{1}]] ({2})", tabs, name, rank);
			string line = string.Format("{0}{1}{0}", tabs, wikiedName, tabs);

			Console.WriteLine( line );

			if (rules != null && rules.includes.ContainsKey(name)) {
				string includesLine = "Includes " + rules.includes[name] + ".";
				Console.WriteLine( includesLine );
			}

			int divide = 24; // don't split if less than 24 bi/tris 
			//TODO: check if there's a lot of solo items and group those together, each with a (family) suffix

			int childBitris = DeepBitriCount(divide);

			bool forceDivide = (rules != null && rules.forceSplit.Contains(name));
			// no point breaking up family into genera
			bool doDivide = forceDivide || (childBitris > divide && children.Count > 0 && rank != "family" && rank != "genus" && rank != "species");

			if (doDivide) {
				foreach (var child in children) {
					if (child.name == "Not assigned") {
						child.name = "ZZZZZ Not assigned"; // "ZZZZZ " for sorting. removed later
					}
				}

				var sortedChildren = from child in children orderby child.name select child; 

				foreach (var child in sortedChildren) {
					child.PrettyPrint(depth + 1);
				}
			} else {
				/*
				foreach (var binom in AllBitrisDeep()) {
					//string tabs2 = new string('*', depth + 1);
					//Console.WriteLine(tabs2 + "''[[" + binom + "]]''");
					Console.WriteLine("''[[" + binom + "]]''"); //TODO: add commas between items
				}
				*/

				//TODO: format subsp. properly 

				//comma separated:
				//string binoms = AllBitrisDeep().Select(binom => "''[[" + Altname(binom) + "]]''").JoinStrings(", ");

				//list:
				// "{{columns-list|4;font-style:italic|" // https://en.wikipedia.org/wiki/IUCN_Red_List_Critically_Endangered_species_(Animalia)
				string cols_start = "{{columns-list|3|"; // \n
				string cols_end = "}}";

				string binoms = cols_start
					+ AllBitrisDeep().OrderBy(bt => bt).Select(binom => "*" + FormatBiTri(binom) + "").JoinStrings("\n")
				                + cols_end;

				Console.WriteLine(binoms);
			}

		}

		public string FormatBiTri(string bitri) {
			string nameInWiki = null;
			if (rules != null && rules.taxonCommonName.ContainsKey(bitri)) {
				nameInWiki = rules.taxonCommonName[bitri].UpperCaseFirstChar();
			} else {
				nameInWiki = BeastieBot.Instance().PageNameInWiki(bitri);
			}

			if (!string.IsNullOrEmpty(nameInWiki) && nameInWiki != bitri) {
				//TODO: check if not redirected to another binom
				//TODO: check if not redirected to a more general taxon

				if (nameInWiki.Contains(" (")) {
					// remove " (sturgeon)" from "Beluga (sturgeon)" etc
					nameInWiki = nameInWiki.Substring(0, nameInWiki.IndexOf(" ("));
				}

				// stop redirects from species to genus, or subspecies to species
				if (bitri.Contains(' ') && nameInWiki.Length < bitri.Length && bitri.StartsWith(nameInWiki)) {
					//TODO: more sophisticated checking (i.e. check the wiki)
					nameInWiki = null;
					//TODO: warn user
				}
			}

			if (!string.IsNullOrEmpty(nameInWiki) && nameInWiki != bitri) {
				return string.Format("[[{0}|{1}]]", bitri, nameInWiki);
			} else {
				return string.Format("''[[{0}]]''", bitri);
			}

		}

		public List<string> AllBitrisDeep(List<string> bitrisList = null) {
			if (bitrisList == null) {
				bitrisList = new List<string>();
			}
			bitrisList.AddRange(this.bitris);

			foreach (var child in children) {
				child.AllBitrisDeep(bitrisList);
			}

			return bitrisList;
		}

		public TaxonNode FindChild(string qrank, string qname) {
			//TODO: search within ranks if plausably there
			foreach (var child in children) {
				if (child.name == qname) {
					if (child.rank == qrank) {
						return child;
					} else {
						Console.Error.WriteLine("Weirdness finding {0}. Expected Rank: {1} Found Rank: {2}", name, rank, child.rank);
						return null;
						//return child; // return it anyway?
					}
				}
			}
			return null;
		}

		/**
		 * Count the number of bi/trinomials below
		 */
		int DeepBitriCount(int max = int.MaxValue) {
			int total = 0;
			total += bitris.Count;

			foreach (var child in children) {
				total += child.DeepBitriCount(max);
				if (total > max)
					return total;
			}

			return total;
		}
	}
}

