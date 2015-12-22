using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	public class IUCNTaxonLadder
	{
		public List<string> ranks = new List<string>(); // kingdom to lowest.
		public Dictionary<string,string> rankName = new Dictionary<string, string>(); // and other details: authority, "infraspecific rank", "infraspecific name", "stock/subpopulation"

		public IUCNTaxonLadder() {
		}

		public void Add(string detail, string name) {
			rankName.Add(detail, name.Trim());
		}

		public void AddFromTop(string rank, string name) {
			ranks.Add(rank);
			name = name.Trim();
			if (rank == "kingdom" || rank == "phylum" || rank == "class" || rank == "order" || rank == "family") {
				name = name.TitleCaseOneWord(); // ToLowerInvariant();
			}
			rankName.Add(rank, name);
		}

		public void InsertBelow(string belowThisRank, string newRank, string newTaxonName) {
			//TODO: fix case and trim like AddFromText
			int index = ranks.IndexOf(belowThisRank);
			if (index < 0) {
				//TODO: throw error or warning or something

				Console.Error.WriteLine("Insert Below failed. Couldn't find rank to insert below: '" + belowThisRank + "'");
				Console.Error.WriteLine("Ranks: " + ranks.JoinStrings(", "));
				return;
			}
			ranks.Insert(index, newRank);
			rankName.Add(newRank, newTaxonName);

			//Console.Error.WriteLine("added a rank: " + this); // debug
		}

		public override string ToString() {
			return string.Format("[TaxonDetails ranks:{0} - rankName:{1}]", 
				ranks.JoinStrings(", "),
				rankName.Select(r => r.Key + ":'" + r.Value + "'").JoinStrings(", ")
			);
		}

		/***
		 * species name, including subspecies (with rank), and stock/population
		 */
		public string FullSpeciesName() {
			// some weird species have infra-ranks but not species (e.g. sp. nov.)
			string species = "";
			if (!string.IsNullOrEmpty(rankName["species"])) {
				species = string.Format(" {0}", rankName["species"]);
			}


			string infra = "";
			if (!string.IsNullOrEmpty(rankName["infraspecific name"])) {
				if (!string.IsNullOrEmpty(rankName["infraspecific rank"])) {
					infra = string.Format(" {0} {1}", rankName["infraspecific rank"], rankName["infraspecific name"]);
				} else {
					infra = string.Format(" {0}", rankName["infraspecific name"]);
				}
			}

			string pop = "";
			if (!string.IsNullOrEmpty(rankName["stock/subpopulation"])) {
				pop = string.Format(" ({0})", rankName["stock/subpopulation"]);
			}
				
			return string.Format("{0}{1}{2}{3}", rankName["genus"], species, infra, pop);
		}

		public IUCNBitri ExtractBitri() {
			IUCNBitri bitri = new IUCNBitri();

			rankName.TryGetValue("genus", out bitri.genus);

			rankName.TryGetValue("species", out bitri.epithet);

			rankName.TryGetValue("infraspecific rank", out bitri.infrarank);

			rankName.TryGetValue("infraspecific name", out bitri.infraspecies);

			rankName.TryGetValue("stock/subpopulation", out bitri.stockpop);

			//rankName.TryGetValue("red list status", out bitri.StatusString); // out bitri.redlistStatus
            //rankName.TryGetValue("special status", out bitri.specialStatus);

            if (rankName.ContainsKey("special status")) {
                RedStatus special = RedStatusFunctions.New(rankName["special status"]);
                bitri.Status = special;
                //TODO: add redundant error check that "red list status" is CR

            } else if (rankName.ContainsKey("red list status")) {
                RedStatus status = RedStatusFunctions.New(rankName["red list status"]);
                bitri.Status = status;

            } else {
                bitri.Status = RedStatus.None;
            }

            string kingdom;
			rankName.TryGetValue("kingdom", out kingdom);

			if (kingdom == "Animalia") {
				bitri.kingdom = IUCNBitri.Kingdom_IUCN.Animalia;
			} else if (kingdom == "Plantae") {
				bitri.kingdom = IUCNBitri.Kingdom_IUCN.Plantae;
			} // TODO: others

			//Console.WriteLine(bitri.redlistStatus);

			return bitri;
		}
	}
}

