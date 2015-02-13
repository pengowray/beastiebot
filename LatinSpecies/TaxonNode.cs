using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
Acinonyx jubatus ssp. hecki => Acinonyx jubatus hecki (animals only)
Dexteria floridana => Dexteria (monotypic) 
Haplochromis sp. 'parvidens-like'
Lipochromis sp. nov. 'small obesoid'
Epiplatys olbrechtsi ssp. azureus
Oncorhynchus nerka (FRASER RIVER, MIDDLE: Quesnel (summer))

Dremomys rufigenis => Red-cheeked squirrel
Dremomys pyrrhomerus => Red-cheeked squirrel

Epinephelus cifuentesi (Gal�pagos Islands subpopulation)


Subpops:
Centrophorus acus (Western Central Atlantic subpopulation)

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

		List<Bitri> bitris = new List<Bitri>(); // species and lower level

		public bool isMajorRank() {
			return (majorRanks.Contains(rank));
		}

		public TaxonNode() {
		}

		public void Add(TaxonDetails details) {
			if (rank == "top") {
				TaxonNode current = this;

				// check manual insertion of inbetween-taxa
				if (rules != null) {
					foreach (var r in details.rankName) {
						if (rules.below.ContainsKey(r.Value)) {
							string[] newTaxonRank = rules.below[r.Value].Split(new char[]{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
							if (newTaxonRank.Length != 2) {
								Console.Error.WriteLine("Warning: maybe 'below' taxon didn't have a rank or something: " + r.Value);
								continue;
							}
							string newTaxonName = newTaxonRank[0];
							string newRank = newTaxonRank[1];
							details.InsertBelow(r.Key, newRank, newTaxonName);

							//TODO: continue on (or restart process). 
							break; // can't remove or will get error "System.InvalidOperationException: Collection was modified"
						}
					}
				}
			

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
			//bitris.Add(details.FullSpeciesName());
			bitris.Add(details.ExtractBitri());
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

		public void PrettyPrint(TextWriter output, string status = null, int depth = 0) {
			if (output == null) {
				output = Console.Out;
			}

			bool anything = (DeepBitriCount(1, status) > 0);

			if (!anything)
				return;

			string tabs = "";
			if (depth > 0) { // no tabs for top node
				tabs = new string('=', depth);
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
						if (nameInWiki.Contains("species") || nameInWiki.Contains("family") || nameInWiki.Contains(" fishes")) {
							wikiedName = string.Format("[[{0}|{1}]]", name, nameInWiki);
						} else {
							wikiedName = string.Format("[[{0}|{1}]] species", name, nameInWiki);
						}
					}
				}
			}

			//string line = string.Format("{0}[[{1}]] ({2})", tabs, name, rank);
			string line = string.Format("{0}{1}{0}", tabs, wikiedName, tabs);

			output.WriteLine(line);

			if (rules != null && rules.includes.ContainsKey(name)) {
				string includesLine = "Includes " + rules.includes[name] + ".";
				output.WriteLine( includesLine );
			}

			int divide = 24; // don't split if less than 24 bi/tris 
			//TODO: check if there's a lot of solo items and group those together, each with a (family) suffix

			int childBitris = DeepBitriCount(divide, status);

			//if (children.Count == 1) {} // jump to child without displaying it

			bool forceDivide = (rules != null && rules.forceSplit.Contains(name));
			// no point breaking up family into genera
			//TODO: don't check if "family", check below ranks are genera
			bool doDivide = forceDivide || (childBitris > divide && children.Count > 0 && rank != "family" && rank != "genus" && rank != "species");

			if (doDivide) {
				foreach (var child in children) {
					if (child.name == "Not assigned") {
						child.name = "ZZZZZ Not assigned"; // "ZZZZZ " for sorting. removed later
					}
				}

				var sortedChildren = from child in children orderby child.name select child; 

				foreach (var child in sortedChildren) {
					child.PrettyPrint(output, status, depth + 1);
				}
			} else {
				//TODO: format subsp. properly 

				//comma separated:
				//string binoms = AllBitrisDeep().Select(binom => "''[[" + Altname(binom) + "]]''").JoinStrings(", ");

				//list:
				// "{{columns-list|4;font-style:italic|" // https://en.wikipedia.org/wiki/IUCN_Red_List_Critically_Endangered_species_(Animalia)
				string cols_start = "{{columns-list|3|"; // \n
				string cols_end = "}}";

				//TODO: order by: get stock/pops to the end 

				string binoms = cols_start
					+ AllBitrisDeep()
					.Where(bt => string.IsNullOrEmpty(status) || bt.redlistStatus == status)
					.OrderBy(bt => bt.FullName())
					.Select(binom => "*" + FormatBitri(binom))
					.JoinStrings("\n")
				    + cols_end;

				output.WriteLine(binoms);
			}

		}

		public string FormatBitri(Bitri bitri) {
			string commonName = null;
			string wikiPage = null;
			string basicName = bitri.BasicName();

			if (rules != null && rules.taxonCommonName.ContainsKey(basicName)) {
				commonName = rules.taxonCommonName[basicName].UpperCaseFirstChar();
			} else {
				commonName = BeastieBot.Instance().PageNameInWiki(basicName);
				wikiPage = commonName;

				//TODO: do a better check that page isn't just the genus etc

				if (!string.IsNullOrEmpty(commonName) && commonName != basicName) {
					//TODO: check if not redirected to another binom
					//TODO: check if not redirected to a more general taxon

					if (commonName.Contains(" (")) {
						// remove " (sturgeon)" from "Beluga (sturgeon)" etc
						commonName = commonName.Substring(0, commonName.IndexOf(" ("));
					}

					// stop redirects from species to genus
					if (commonName == bitri.genus) {
						//TODO: more sophisticated checking (i.e. check the wiki)
						Console.Error.WriteLine("Note: '{0}' common name not used because it is the genus: {1}", bitri.FullName(), commonName);
						commonName = null;
					}

					// stop redirect from subspecies to species or to genus
					if (bitri.isTrinomial && commonName == bitri.ShortBinomial()) {
						//TODO: more sophisticated checking (i.e. check the wiki)
						Console.Error.WriteLine("Note: '{0}' common name not used because it's not the full trinomial: {1}", bitri.FullName(), commonName);
						commonName = null;
					}
				}

			}

			// link to "Anura (frog)" not "Anura" (disambig)
			string wikilink = basicName;
			if (rules != null && rules.wikilink.ContainsKey(basicName)) {
				wikilink = rules.wikilink[basicName];
			}

			//TODO: list subspecies separately?
			bool needSubspWarning = bitri.isTrinomial && (commonName != null && commonName != basicName);
			string subspWarning = needSubspWarning  ? " (subspecies)" : "";

			string pop = bitri.isStockpop ? " (" + bitri.stockpop + ")" : "";

			if (!string.IsNullOrEmpty(commonName) && commonName != wikilink) {
				return string.Format("[[{0}|{1}]]{2}{3}", wikilink, commonName, subspWarning, pop);
			} else {
				return string.Format("''[[{0}]]''{1}{2}", wikilink, subspWarning, pop);
			}
		}

		public List<Bitri> AllBitrisDeep(List<Bitri> bitrisList = null) {
			if (bitrisList == null) {
				bitrisList = new List<Bitri>();
			}
			bitrisList.AddRange(this.bitris);

			foreach (var child in children) {
				child.AllBitrisDeep(bitrisList);
			}

			return bitrisList;
		}

		public TaxonNode FindChildDeep(string qname) {
			// search deep, but breadth first, kinda not really
			string lowername = qname.ToLowerInvariant();
			foreach (var c1 in children) {
				//if (child.name == qname) {
				if (c1.name.ToLowerInvariant() == lowername)  {
					return c1;
				}
			}

			foreach (var c2 in children) {
				var result = c2.FindChildDeep(qname);
				if (result != null) {
					return result;
				}
			}

			return null;
		}

		public TaxonNode FindChild(string qname) {
			return FindChild(null, qname);
		}

		public TaxonNode FindChild(string qrank, string qname) {
			//TODO: search within ranks if plausably there
			foreach (var child in children) {
				if (child.name == qname) {
					if (qrank == null || child.rank == qrank) {
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
		int DeepBitriCount(int max = int.MaxValue, string statusFilter = null) {
			int total = 0;
			if (string.IsNullOrEmpty(statusFilter)) {
				total += bitris.Count;
			} else {
				total += bitris.Where(b => b.redlistStatus == statusFilter).Count();
			}

			foreach (var child in children) {
				total += child.DeepBitriCount(max, statusFilter);
				if (total > max)
					return total;
			}

			return total;
		}
	}
}

