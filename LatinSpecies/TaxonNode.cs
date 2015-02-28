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
All zero species in Odobenidae which have been assessed are critically endangered. 
(done) Gecarcinucidae (many redirects to genus, not monotypic). ALso: Parastacidae, and Isopoda. e.g. Ceylonthelphusa sanguinea, Thermosphaeroma cavicauda
How to handle?: 
(done) Acinonyx jubatus ssp. hecki => Acinonyx jubatus hecki (animals only)
(avoided) Dexteria floridana => Dexteria (monotypic) 
(ok) Haplochromis sp. 'parvidens-like'
(ok) Lipochromis sp. nov. 'small obesoid'
Epiplatys olbrechtsi ssp. azureus
(done, hidden) Oncorhynchus nerka (FRASER RIVER, MIDDLE: Quesnel (summer))
Gastonia mauritiana => Polyscias maraisiana
Leucocharis pancheri => Leucocharis pancheri

** Dremomys rufigenis => Red-cheeked squirrel
** Dremomys pyrrhomerus => Red-cheeked squirrel

Walrus => type species in taxobox, not binomial

?? Okapia johnstoni=>Okapi - Redirect is to another binoimal

(ok?) Dipodomys margaritae=>Margarita Island kangaroo rat - Redirect is not to a bionomial (bi -> tri is caught)

(ok) Epinephelus cifuentesi (Gal�pagos vs Galápagos) // RedList csv is ANSI / Windows 1252, not Unicode

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

Panthera pardus nimr = Arabian leopard
Pennatomys nivalis => Pennatomys // only species in the genus


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

		List<IUCNBitri> bitris = new List<IUCNBitri>(); // species and lower level

		public bool isMajorRank() {
			return (majorRanks.Contains(rank));
		}

		public TaxonNode() {
		}

		public void Add(IUCNTaxonLadder details) {
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

		/**
		 * IUCN Red List Index of species survival
		 * 
		 * @returns 1 if all speceies are LC, and 0 if all extinct. 
		 * Ignores subspecies and stocks/subpopulations
		 */
		public double RLI() {
			/*
			Put simply, the number of species in each Red List Category
			is multiplied by the Category weight (which ranges from 0 for
			Least Concern, 1 for Near Threatened, 2 for Vulnerable, 3 for
			Endangered, 4 for Critically Endangered and 5 for Extinct in the
			Wild and Extinct). These products are summed, divided by the
			maximum possible product (the number of species multiplied by
			the maximum weight), and subtracted from one. This produces
			an index that ranges from 0 to 1 (see below).
			-- https://portals.iucn.org/library/sites/library/files/documents/2009-001.pdf
			*/

			var valid = DeepBitris().Where(bt => !bt.isTrinomial && !bt.isStockpop && bt.CategoryWeight() != null);
			int numerator = (int)valid.Sum(bt => bt.CategoryWeight());
			int denominator = valid.Count() * 5;
			double rli = 1 - ((double)numerator / (double)denominator);

			return rli;
		}

		public string StocksOrSubpopsText(int count, bool newspaperNumbers = false) { 
			// note: assessing whether to use stock or subpopulation from this TaxonNode, but
			// count may be for a subset of this TaxonNode's children, e.g. one species

			string plural = "";
			if (count > 1)
				plural = "s";

			string countText = count.ToString();
			if (newspaperNumbers) {
				countText = count.NewspaperNumber();
			}

			if (DeepBitris().Any(b => b.isStockpop)) {
				if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("subpopulation"))) {
					return countText + "&nbsp;subpopulation" + plural;

				} else if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("stock"))) {
					return countText  + "&nbsp;stock" + plural;

				} else {
					if (count > 1) {
						if (newspaperNumbers) {
							return countText + " subpopulations or stocks";
						} else {
							return countText + "&nbsp;subpopulations/stocks";
						}
					} else {
						if (newspaperNumbers) {
							return countText + " subpopulation or stock";
						} else {
							return countText + "&nbsp;subpopulation/stock";
						}
					}
				}

			} else {
				return null;
			}
		}

		public string StocksOrSubpopsHeading() {
			int pops = DeepBitriCountWhere(b => b.isStockpop);
			if (pops == 0)
				return string.Empty;

			if (DeepBitris().Any(b => b.isStockpop)) {
				if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("subpopulation"))) {
					return "Subpopulations";
				} else if (DeepBitris().All(b => !b.isStockpop || b.stockpop.ToLowerInvariant().Contains("stock"))) {
					return "Stocks";
				} else {
					return "Subpopulations and stocks";
				}

			} else {
				return null;
			}
		}

		public void AddSpeciesChild(IUCNTaxonLadder details) {
			//bitris.Add(details.FullSpeciesName());
			bitris.Add(details.ExtractBitri());
		}

		public void PrettyPrint(TextWriter output, string status = null, int depth = 0) {
			if (output == null) {
				output = Console.Out;
			}

			bool anything = (DeepBitriCount(status, 1) > 0);

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
				string commonName = null;
				if (rules != null && rules.taxonCommonName.ContainsKey(name)) {
					commonName = rules.taxonCommonName[name].UpperCaseFirstChar();
				} else {
					commonName = BeastieBot.Instance().TaxaCommonNameFromWiki(name);
				}

				if (!string.IsNullOrEmpty(commonName)) {

					// fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
					commonName.Replace("  ", " "); 

					if (commonName.Contains(" (")) {
						// remove " (insect)" from "Cricket (insect)"
						commonName = commonName.Substring(0, commonName.IndexOf(" ("));
					}
					if (commonName != name) {
						if (commonName.Contains("species") || commonName.Contains("family") || commonName.Contains(" fishes")) {
							wikiedName = string.Format("[[{0}|{1}]]", name, commonName);
						} else {
							wikiedName = string.Format("[[{0}|{1}]] species", name, commonName);
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

			PrintStats();


			int divide = 27; // don't split if less than 27 bi/tris. 
			int oneDivide = 20; // allow one split if over 20 (to cause CR bats to split, but not new world monkeys.. very arbitrary)
			//TODO: check if the 2 children have anything that will be displayed

			//TODO: check if there's a lot of solo items and group those together, each with a (family) suffix

			int childBitris = DeepBitriCount(status, divide);

			//if (children.Count == 1) {} // jump to child without displaying it

			bool forceDivide = (rules != null && rules.forceSplit.Contains(name));
			bool dividableRank = (rank != "family" && rank != "genus" && rank != "species");
			// no point breaking up family into genera
			//TODO: don't check if "family", check below ranks are genera
			bool doDivide = forceDivide || 
				(childBitris > divide && children.Count > 2 && dividableRank) ||
				(childBitris > oneDivide && children.Count == 2 && dividableRank);

			if (doDivide) {
				foreach (var ch in children) {
					if (ch.name == "Not assigned") {
						ch.name = "ZZZZZ Not assigned"; // "ZZZZZ " for sorting. removed later
					}
				}

				//var sortedChildren = from child in children orderby child.name select child; 
				var sortedChildren = from child in children orderby child.RLI() select child;  // sort by redlist indicator (extinction risk)

				foreach (var ch in sortedChildren) {
					ch.PrettyPrint(output, status, depth + 1);
				}
			} else {
				//TODO: format subsp. properly 

				//comma separated:
				//string binoms = AllBitrisDeep().Select(binom => "''[[" + Altname(binom) + "]]''").JoinStrings(", ");

				//list:
				// "{{columns-list|4;font-style:italic|" // https://en.wikipedia.org/wiki/IUCN_Red_List_Critically_Endangered_species_(Animalia)

				//TODO: order by: get stock/pops to the end 

				List<IUCNBitri> deepBitriList;
				if (!string.IsNullOrEmpty(status)) {
					deepBitriList = AllBitrisDeepWhere(bt => bt.redlistStatus == status);
				} else {
					deepBitriList = AllBitrisDeepWhere();
				}

				bool anyBinoms = deepBitriList.Any(bt => !bt.isStockpop && !bt.isTrinomial);
				bool anySubspecies = deepBitriList.Any(bt => bt.isTrinomial && !bt.isStockpop);
				bool anyStockPops = deepBitriList.Any(bt => bt.isStockpop);
				bool includeStatus = string.IsNullOrEmpty(status);

				if (anyBinoms) {
					if (anySubspecies || anyStockPops) {
						output.WriteLine("\n'''Species'''");
					}
					output.WriteLine(FormatBitriList(deepBitriList.Where(bt => !bt.isStockpop && !bt.isTrinomial), includeStatus));
				} else {
					output.WriteLine(string.Empty);
				}

				if (anySubspecies) {
					//TODO: plant varieties and shit
					output.WriteLine("'''Subspecies'''");
					output.WriteLine(FormatBitriList(deepBitriList.Where(bt => bt.isTrinomial && !bt.isStockpop), includeStatus));
				}

				if (anyStockPops) {
					//output.WriteLine("'''Stocks and populations'''");
					output.WriteLine("'''" + StocksOrSubpopsHeading() + "'''");
					//output.WriteLine(FormatBitriList(deepBitriList.Where(bt => bt.isStockpop), includeStatus));
					var groups = deepBitriList.Where(bt => bt.isStockpop).GroupBy(b => b.BasicName()).OrderBy(b => b.Key);
					var grouped = groups.Select(g => g.First().CloneMultistockpop(StocksOrSubpopsText(g.Count())));
					//foreach (var group in groups) {
						//output.WriteLine("* " + FormatBitri(group.First(), false, StocksOrSubpopsText(group.Count()) ));
					//}
					output.WriteLine(FormatBitriList(grouped, false, 3));
				}

			}

		}
			

		public string FormatBitriList(IEnumerable<IUCNBitri> bitris, bool includeStatus = false, int columns = 3) {
			if (bitris.Count() == 0)
				return string.Empty;

			string cols_start = "{{columns-list|" + columns + "|"; // \n
			string cols_end = "}}";

			if (bitris.Count() < columns) {
				cols_start = string.Empty;
				cols_end = string.Empty;
			}

			return cols_start +
				bitris.OrderBy(bt => bt.FullName())
				.Select(binom => "*" + FormatBitri(binom, includeStatus))
				.JoinStrings("\n")
				+ cols_end;
		}

		public string FormatBitri(IUCNBitri bitri, bool includeStatus = false) {
			string commonName = null;
			string wikiPage = null;
			string basicName = bitri.BasicName();

			if (rules != null && rules.taxonCommonName.ContainsKey(basicName)) {
				commonName = rules.taxonCommonName[basicName].UpperCaseFirstChar();

			} else {
				//commonName = BeastieBot.Instance().PageNameInWiki(basicName);
				commonName = BeastieBot.Instance().CommonNameFromWiki(bitri);
				wikiPage = commonName;

				if (!string.IsNullOrEmpty(commonName) && commonName != basicName) {
					if (commonName.Contains(" (")) {
						// remove " (sturgeon)" from "Beluga (sturgeon)" etc
						commonName = commonName.Substring(0, commonName.IndexOf(" ("));
					}
				}

			}

			//TODO FIXME XXXXXXXXXXXXX: temporarily disable trinomial common names
			//if (bitri.isTrinomial) {
			//	commonName = null;
			//}

			// link to "Anura (frog)" not "Anura" (disambig)
			string wikilink = basicName;
			if (rules != null && rules.wikilink.ContainsKey(basicName)) {
				wikilink = rules.wikilink[basicName];
			}

			//now lists subspecies separately, so no need for "warning".
			//bool needSubspWarning = bitri.isTrinomial && (commonName != null && commonName != basicName);
			//string subspWarning = needSubspWarning  ? " (subspecies)" : "";
			string subspWarning = "";

			string pop = string.Empty;
			if (bitri.isStockpop) {
				pop = " (" + bitri.stockpop + ")";
			}

			string special = string.Empty;
			if (bitri.specialStatus != null) {
				special = " (" + (bitri.specialStatus == "CR(PE)" ? "possibly&nbsp;extinct" : "possibly extinct in the wild") + ")";
			}

			string extinct = bitri.redlistStatus == "EX" ? "{{Extinct}}" : "";
			string status = (includeStatus && bitri.redlistStatus != "" && bitri.redlistStatus != "EX") ? " " + bitri.redlistStatus : "";

			if (!string.IsNullOrEmpty(commonName) && commonName != wikilink) {
				return string.Format("{0}[[{1}|{2}]]{3}{4}{5}{6}", extinct, wikilink, commonName, subspWarning, pop, status, special);
			} else {
				return string.Format("{0}''[[{1}]]''{2}{3}{4}{5}", extinct, wikilink, subspWarning, pop, status, special);
			}
		}

		void PrintStats() {
			// statistics
			int cr_count = DeepBiCount("CR");
			int all_count = DeepBitriCountWhere(b => !b.isStockpop && !b.isTrinomial && b.redlistStatus != "DD"); // all assessed
			int threatened_or_extinct_count = DeepBitriCountWhere(b => !b.isStockpop && !b.isTrinomial && b.isThreatenedOrExtinct);
			int cr_pops_count = DeepBitriCountWhere(b => b.isStockpop && b.redlistStatus == "CR");
			int cr_infras_count = DeepBitriCountWhere(b => !b.isStockpop && b.isTrinomial && b.redlistStatus == "CR");
			int dd_count = DeepBitriCountWhere(b => !b.isStockpop && !b.isTrinomial && b.redlistStatus == "DD");

			//TODO: chart
			//string statusGraph = DeepBitriStatusGraph();
			//output.WriteLine(statusGraph);

			//TODO:
			//0 species have been assessed as critically endangered of the 2 assessed in Proboscidea. (has a subspecies)

			/*
			if (all_count == cr_count) {
				if (all_count == 1) {
					// only one species in this category
				} else if (all_count == 2) {
					output.WriteLine("Both species which have been assessed are critically endangered."); 
					// all_count.NewspaperNumber());
				} else {
					output.WriteLine("All {0} species in {1} which have been assessed are critically endangered.", 
						all_count.NewspaperNumber(), name);
				}
			} else if (threatened_or_extinct_count != cr_count) {
				output.WriteLine("{0} species {1} critically endangered of the {2} in {3} which {4} assessed.",
					cr_count.NewspaperNumber().UpperCaseFirstChar(),
					(cr_count == 1 ? "is" : "are"),
					//threatened_count.NewspaperNumber(),
					all_count.NewspaperNumber(), 
					name,
					(all_count == 1 ? "has been" : "have been")
				);

			} else {
				output.WriteLine("There {0} {1} critically endangered {2} species of the {3} assessed.", 
					(cr_count == 1 ? "is" : "are"),
					cr_count.NewspaperNumber(), // .UpperCaseFirstChar(),
					name,
					all_count.NewspaperNumber()
				);
			}


			if (cr_infras_count > 0 && cr_pops_count > 0) {
				//TODO: varieties or whatever for plants (currently assumes animal-style subspecies)
				output.WriteLine("There are also {0} subspecies and {1} which are critically endangered.",
					cr_infras_count.NewspaperNumber(),
					StocksOrSubpopsText(cr_pops_count, true),
					(cr_pops_count == 1 ? "" : "s")
				);


			} else {
				//TODO: varieties or whatever for plants (currently assumes animal-style subspecies)
				if (cr_infras_count > 0) {
					output.WriteLine("There {1} also {0} subspecies which {1} critically endangered.",
						cr_infras_count.NewspaperNumber(),
						(cr_infras_count == 1 ? "is" : "are"));
				}

				if (cr_pops_count > 0) {
					output.WriteLine("There {1} also {0} which {1} critically endangered.",
						StocksOrSubpopsText(cr_pops_count, true),
						(cr_pops_count == 1 ? "is" : "are"),
						(cr_pops_count == 1 ? "" : "s"));
				}
			}

			if (dd_count > 0) {
				output.WriteLine("{0} other species in this group {1} [[Data deficient|insufficient information]] for a proper assessment of conservation status.",
					dd_count.NewspaperNumber().UpperCaseFirstChar(),
					(dd_count == 1 ? "has" : "have"));
			}
			*/

		}

		public List<IUCNBitri> AllBitrisDeepWhere(Func<IUCNBitri,bool> whereFn = null, List<IUCNBitri> bitrisList = null) {
			if (bitrisList == null) {
				bitrisList = new List<IUCNBitri>();
			}
			if (whereFn == null) {
				bitrisList.AddRange(bitris);
			} else {
				bitrisList.AddRange(bitris.Where(whereFn));
			}

			foreach (var child in children) {
				child.AllBitrisDeepWhere(whereFn, bitrisList);
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
		int DeepBitriCount(string statusFilter = null, int max = int.MaxValue) {
			if (string.IsNullOrEmpty(statusFilter)) {
				return DeepBitriCountWhere(null, max);
			} else {
				//total += bitris.Where(b => b.redlistStatus == statusFilter).Count();
				return DeepBitriCountWhere(b => b.redlistStatus == statusFilter, max);
			}
		}

		// count binomials only
		int DeepBiCount(string statusFilter = null, int max = int.MaxValue) {
			if (string.IsNullOrEmpty(statusFilter)) {
				return DeepBitriCountWhere(b => !b.isTrinomial && !b.isStockpop);
			} else {
				return DeepBitriCountWhere(b => !b.isTrinomial && !b.isStockpop && b.redlistStatus == statusFilter);
			}
		}

		IEnumerable<IUCNBitri> DeepBitris() {
			foreach (var bt in bitris) {
				yield return bt;
			}

			foreach (var child in children) {
				foreach (var bt in child.DeepBitris()) {
					yield return bt;
				}
			}
		}

		int DeepBitriCountWhere(Func<IUCNBitri, bool> whereFn, int max = int.MaxValue) {
			int total = 0;
			if (whereFn == null) {
				total += bitris.Count;
			} else {
				total += bitris.Where(whereFn).Count();
			}

			foreach (var child in children) {
				total += child.DeepBitriCountWhere(whereFn, max);
				if (total > max)
					return total;
			}

			return total;
		}


		Dictionary<string, int> DeepBitriStatusCountWhere(Func<IUCNBitri, bool> whereFn, Dictionary<string, int> statuses = null) {
			if (statuses == null)
				statuses = new Dictionary<string, int>();

			if (whereFn != null) {
				foreach (var bitri in bitris.Where(whereFn)) {
					if (bitri.redlistStatus != null) {
						statuses.AddCount(bitri.redlistStatus, 1);
					}
				}
			} else {
				foreach (var bitri in bitris) {
					if (bitri.redlistStatus != null) {
						statuses.AddCount(bitri.redlistStatus, 1);
					}
				}

			}

			foreach (var child in children) {
				child.DeepBitriStatusCountWhere(whereFn, statuses);
			}

			return statuses;
		}

		string DeepBitriStatusGraph() {
			var statuses = DeepBitriStatusCountWhere(bt => !bt.isTrinomial && !bt.isStockpop);


			string chartTemplate = @"{{ #invoke:Chart | bar chart
| height = 250
| width = 300
| stack = 1
| group 1 = {0}" + // 40 : 50 : 60 : 20
@"| colors = green : yellow : orange
| group names = {1}" + // Apple : Banana : Orange
@"| units suffix = Kg
| x legends = Conservation status
}}";
			string chart = string.Format(chartTemplate, 
				statuses.Keys.JoinStrings(" : "),
				statuses.Values.Select(i => i.ToString()).JoinStrings(" : ")
			);

			return chart;

		}


	}
}

