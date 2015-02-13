using System;
using System.Collections;
using System.Linq;

namespace beastie {
	// A binomial or trinomial, with optional stock/population.
	public class Bitri
	{
		//enum Kingdom { None, Plant, Animal, Fungi } // etc...

		//▿

		//enum type { None, binom, trinom }
		public bool isStockpop {
			get {
				return !string.IsNullOrEmpty(stockpop);
			}
		}

		public bool hasSubgenus {
			get { 
				return !string.IsNullOrEmpty(subgenus);
			}
		}

		public bool isTrinomial {
			get {
				return (!string.IsNullOrEmpty(epithet) && !string.IsNullOrEmpty(infraspecies));
			}
		}

		public bool isVulnerable {
			get {
				string[] vulnerable = new string[] { "CR", "EN", "VU", "PE", "PW", "PEW" };
				return (vulnerable.Contains(redlistStatus));
			}
		}

		// Ootaxa: Template:Oobox
		// Ichnotaxa: Template:Ichnobox
		// Excavata (Domain: Eukaryota)
		// Rhizaria (unranked) (Domain: Eukaryota)
		// Chromalveolata (Domain: Eukaryota) (polyphyletic)
		enum Kingdom_Taxobox { None, Animalia, Archaeplastida, Fungi, Chromalveolata, Rhizaria, Excavata, Amoebozoa, Bacteria, Archaea, Viruses, incertae_sedis, Ichnotaxa, Ootaxa }
		enum Kingdom_COL { None, Animalia, Archaea, Bacteria, Chromista, Fungi, Plantae, Protozoa, Viruses }
		public enum Kingdom_IUCN { None, Animalia, Bacteria, Chromista, Fungi, Plantae, Protozoa }

		//{{Automatic taxobox
		//{{Speciesbox

		public Kingdom_IUCN kingdom;

		public string genus;
		public string subgenus;
		public string epithet;
		public string infrarank; // infraspecific rank, e.g. subsp. var. 
		public string connecting_term; // from above
		public string infraspecies; // e.g. subspecies or variety

		public string stockpop; // stock/population

		public string redlistStatus; // IUCN redlist status (e.g. EN)

		public Bitri() {
		}

		public bool isInfrarankVisible {
			get {
				if (string.IsNullOrEmpty(infraspecies)) // no infraspecies, no infrarank
					return false;

				if (string.IsNullOrEmpty(infrarank))
					return false;

				if (kingdom == Kingdom_IUCN.Animalia && infrarank == "ssp.")
					return false;

				return true;
			}
		}

		/**
		 * Excludes status
		 * Excludes "ssp." infrarank label for animals
		 * 
		 * used for matching IUCN names with TaxonDisplayRules
		 */
		public string BasicName() {
			// copied from TaxonDetails.FullSpeciesName()
			// some weird species have infra-ranks but not epithets (e.g. sp. nov.)
			string speciesString = "";
			if (!string.IsNullOrEmpty(epithet)) {
				speciesString = string.Format(" {0}", epithet);
			}

			string infraString = "";
			if (!string.IsNullOrEmpty(infraspecies)) {
				if (isInfrarankVisible) {
					infraString = string.Format(" {0} {1}", infrarank, infraspecies);
				} else {
					infraString = string.Format(" {0}", infraspecies);
				}
			}

			return string.Format("{0}{1}{2}", genus, speciesString, infraString);
		}

		/** includes stock/pop. For debug.
		TODO: include subgenus
		 */
		public string FullName() {

			//string pop = "";
			if (!string.IsNullOrEmpty(stockpop)) {
				return string.Format("{0} ({1})", BasicName(), stockpop);
			} else {
				return BasicName();
			}
		}

		public string ShortBinomial() {
			// some weird species have infra-ranks but not epithets (e.g. sp. nov.)
			string speciesString = "";
			if (!string.IsNullOrEmpty(epithet)) {
				speciesString = string.Format(" {0}", epithet);
			} else {
				// only use infraspecies + infrarank if missing epithet
				if (!string.IsNullOrEmpty(infraspecies)) {
					if (!string.IsNullOrEmpty(infrarank)) {
						speciesString = string.Format(" {0} {1}", infrarank, infraspecies);
					} else {
						speciesString = string.Format(" {0}", infraspecies);
					}
				}
			}

			return string.Format("{0}{1}", genus, speciesString);
		}
	}
}

