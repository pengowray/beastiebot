using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {
	public class TaxonDetails
	{
		public List<string> ranks = new List<string>(); // kingdom to lowest.
		public Dictionary<string,string> rankName = new Dictionary<string, string>(); // and other details: authority, "infraspecific rank", "infraspecific name", "stock/subpopulation"

		public TaxonDetails() {
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
	}
}

