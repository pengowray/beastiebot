using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace beastie {

    // A binomial or trinomial, with optional stock/population, and threatened status
    // TODO: make into a composite containing a generic Bitri (+ stockpop + status + special status)
    // TODO regions?

    public class IUCNBitri : ICloneable // was: Bitri
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

        // binomial, not a trinomial, not a stockpop.
        public bool isSpecies
        {
            get
            {
                return !isStockpop && !isTrinomial;
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

        public Kingdom_IUCN kingdom;

        public string genus;
        public string subgenus;
        public string epithet;
        public string infrarank; // infraspecific rank, e.g. subsp. var. 
        public string connecting_term; // from above
        public string infraspecies; // e.g. subspecies or variety

        public string stockpop; // stock/subpopulation
        public bool multiStockpop; // if this Bitri represents multiple Stocks/Subpopulation. If true, "stockpop" must contain a description, e.g. "1 stock or subpopulation" or "3 subpopulations", and redlistStatus should only be set if all members are the same

        public string CommonNameEng; // list of comma separated common names from the IUCN

        public string FirstCommonNameEng() {
            if (string.IsNullOrEmpty(CommonNameEng))
                return null;

            var names = CommonNameEng.Split(new char[] { ',' }, 2);

            if (names.Length >= 1) {
                return names[0];
            }

            return null;
        }

        public string[] CommonNamesEng() {
            if (string.IsNullOrEmpty(CommonNameEng))
                return null;

            return CommonNameEng.Split(new char[] { ',' }).Select(m => m.Trim()).Where(m => m != string.Empty).ToArray();
        }

        public string BestCommonNameEng() {
            string[] names = CommonNamesEng();
            if (names == null)
                return null;

            if (names.Length == 1)
                return names[0];

            //find a name without any apostrophe (to avoid eponymous names)
            string best = names.Where(n => !n.Contains("'")).FirstOrDefault();
            if (best != null)
                return best;

            return names[0]; // ok just the first one then.
        }


        public RedStatus Status; // should neve be RedStatus.Null. use None or Unknown instead.

        TaxonPage _taxonName; // cached // (use TaxonName instead?)

        public IUCNBitri() {
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
		 * Exclused stock/subpopulation
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
		public IUCNBitri CloneMultistockpop(string stockpopText, bool keepStatus = false) {
			IUCNBitri clone = (IUCNBitri) Clone();
			clone.multiStockpop = true;
			clone.stockpop = stockpopText;
			if (!keepStatus) 
				clone.Status = RedStatus.None;

			return clone;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
		}

        public TaxonPage TaxonName() {
            if (_taxonName == null) {
                _taxonName = BeastieBot.Instance().GetTaxonNamePage(this);
            }
            
            return _taxonName;
        }

    }
}

