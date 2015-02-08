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
//"Not assigned"

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

			//string altname = Altname();
			string altname = Altname(name);

			//string line = string.Format("{0}[[{1}]] ({2})", tabs, name, rank);
			string line = string.Format("{0}[[{1}]] ({2}){0}", tabs, altname, rank);

			Console.WriteLine( line );

			int divide = 32; // don't split if less than 32 bi/tris

			int childBitris = DeepBitriCount(divide);
			// no point breaking up family into genera
			bool doDivide = (childBitris > divide) && children.Count > 0 && rank != "family" && rank != "genus" && rank != "species";

			if (doDivide) {
				foreach (var child in children) {
					//TODO: sort?
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
				string binoms = AllBitrisDeep().Select(binom => "''[[" + Altname(binom) + "]]''").JoinStrings(", ");
				Console.WriteLine(binoms);
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

