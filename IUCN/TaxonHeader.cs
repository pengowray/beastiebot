using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace beastie {
	public class TaxonHeader
	{
		TaxonNode node;

		public string taxon;
		public int depth;

		public string commonName;
        public string commonNameOverride;
		bool weirdCommonName = false; // commonName or override contains the word "family", "fishes" or "species" or is plural
		bool notAssign; // the name of this taxon is literally "Not assigned"

		public string wikiName; // name on wikipedia, e.g. "Cricket (insect)"
		//TODO: wiki name override?

		public string wikiedName; // [[Pholidota|Pangolin]] species
		public string wikiedHeader; // "==[[Pholidota|Pangolin]] species=="

        public string comprises;
        public string includes;
        public string means;

        //public string reportingSentence;

        public TaxonHeader(TaxonNode node, string taxon, int depth, string overrideCommonName = null, string overrideCommonNameWithPlural = null, string comprises = null, string includes = null, string means = null) {
			this.node = node;
			this.taxon = taxon;
			this.depth = depth;
            this.comprises = comprises;
			this.includes = includes;
            this.means = means;

            //TODO: preserve common name / plural / etc and use as needed

            if (taxon == "Not assigned" || taxon == "ZZZZZ Not assigned") {
				wikiedName = "Not assigned";
				notAssign = true;
				//reportingSentence = "This group";
				//reportingSentence = "This group contains {1} {2} {3}";

			} else {
				commonName = null;
                if (overrideCommonNameWithPlural != null) {
                    commonName = overrideCommonNameWithPlural.UpperCaseFirstChar();
                    weirdCommonName = true;

                } else if (overrideCommonName != null) {
                    //reportingNameUpper = overrideCommonName.UpperCaseFirstChar();
                    commonName = overrideCommonName.UpperCaseFirstChar();
                    //wikiedName = commonName; // default to override?

				} else {
					wikiName = BeastieBot.Instance().TaxaCommonNameFromWiki(taxon);
					commonName = wikiName; 
					//reportingNameUpper = commonName;
				}

				if (!string.IsNullOrEmpty(commonName)) {

					// fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
					commonName.Replace("  ", " "); 

					if (commonName.Contains(" (")) {
						// remove " (insect)" from "Cricket (insect)"
						commonName = commonName.Substring(0, commonName.IndexOf(" ("));
					}

					if (commonName != taxon) {
						if (weirdCommonName || commonName.Contains("species") || commonName.Contains("family") || commonName.Contains(" fishes")) {
                            wikiedName = string.Format("[[{0}|{1}]]", taxon, commonName);
							weirdCommonName = true;

						} else {
                            wikiedName = string.Format("[[{0}|{1}]] species", taxon, commonName);

                        }
                    } else {
						wikiedName = "[[" + taxon + "]]";
					}
				} else {
					wikiedName = "[[" + taxon + "]]";

				}

			}

		}

		public string HeadingString() {
			bool noHeader0 = true; // no header for depth 0 (e.g. "Mammal species")
			bool skipDepth1 = true; // don't return "=Pangolin species=", instead "==Pangolin species=="

			if (noHeader0 && depth == 0) {
				return string.Empty;
			}
			
			int headerDepth = depth;

			if (skipDepth1) {
				if (headerDepth >= 1) {
					headerDepth++;
				}
			}

			string tabs = string.Empty;
			tabs = new string('=', headerDepth);

			//string line = string.Format("{0}[[{1}]] ({2})", tabs, name, rank);
			string line = string.Format("{0}{1}{0}", tabs, wikiedName, tabs);

			return line;
		}

        public string GrayText() {
            if (!string.IsNullOrWhiteSpace(means)) {
                return "{{gray|(\"" + means + "\")}}"; // {{gray|("means")}} // note: string.Format turns {{ into {.
            }

            if (!string.IsNullOrWhiteSpace(comprises)) {
                //return string.Format(@"{{gray|{0}}}", comprises);
                return "{{gray|" + comprises + "}}"; // {{gray|comprises}}

            }

            if (!string.IsNullOrWhiteSpace(includes)) {
                return "{{gray|Includes " + includes + "}}"; // {{gray|Includes includes}}
            }

            return null;
        }

        public string VernacularStringLower() {
			if (commonNameOverride != null) {
				return commonNameOverride;
			} else if (commonName != null) {
				//TODO: don't lowercase "American" etc
				//TODO: maybe only lowercase first character?
				return commonName.ToLowerInvariant(); 
			} else {
				return taxon; // .ToLowerInvariant();
			}
		}

		public string PrintStatsBeforeSplit(string status) {
			// {0}=are/is, {1}=number, {2}=status/adjective (e.g. "critically endangered"), {3}=species/subspecies/subpopulation(s)
			// {4}=commonName (lowercased), {5}=taxon
			// "There are 1000 critically endangered pangolin species" => "There {0} {1} {2} pangolin {3}" 
			// "There are 1000 critically endangered pangolin species, and 500 critically endangered subspecies." => "There {0} {1} {2} pangolin {3}" 
			// "Myomorpha contains 1,137 critically endangered species" => "Myomorpha contains {1} {2} {3}"
			// This group contains 1,233 critically endangered species and 23 critically endangered subspecies."
			// This group contains 1,233 critically endangered species, 23 critically endangered subspecies. There are also 4 endangered subpopulations."

			//reportingSentence = "Within the {4} there {0} {2} {3}"; // for weird one containing the text "family"

			if (notAssign)
				return null; // don't bother with special stats for "Not assigned" taxon.


			Dictionary<string, string> codes_en = new Dictionary<string, string>();
			codes_en["CR"] = "critically endangered";

			int all_count = node.DeepBitriCountWhere(b => !b.isStockpop && !b.isTrinomial); // all assessed (including DD)
			int cr_count = node.DeepBiCount(status);
			int cr_infras_count = node.DeepBitriCountWhere(b => !b.isStockpop && b.isTrinomial && b.redlistStatus == status);
			int cr_pops_count = node.DeepBitriCountWhere(b => b.isStockpop && b.redlistStatus == status);

			if (string.IsNullOrWhiteSpace(status)) {
				return null;
			}
			string status_full = codes_en[status];

			bool useCommon = (!string.IsNullOrEmpty(commonName) && (commonName != taxon) && !weirdCommonName);
			string reportingSentence = "";

			string includesClause = string.Empty;
			string includesSentence = string.Empty;
			if (!string.IsNullOrWhiteSpace(includes)) {
				includesClause  = ", which includes " + includes + ",";
				includesSentence = "Includes " + includes + ". "; // not currently used
			}

			if (cr_count > 0 && cr_infras_count == 0) {
				if (useCommon) {

					// There [0=is] [1=one] [2=critically endangered] [3=pangolin] species.
					reportingSentence = includesSentence + "There {0} {1} {2} {3} species. ";

				} else {

					// [5=family] contains [1=one] [2=endangered] species.
					reportingSentence = "{5}{7} contains {1} {2} species. ";
				}
			} else if (cr_count > 0 && cr_infras_count > 0) {
				if (useCommon) {
					// ... [6=six] (ssps.) [2=endangered]
					reportingSentence = "There {0} {1} {2} {3} species and {6} {2} subspecies. ";
				} else {
					reportingSentence = "{5}{7} contains {1} {2} species and {6} {2} subspecies. ";
				}
			} else if (cr_count == 0 && cr_infras_count > 0) {
				if (useCommon) {
					// There [0=is] [1=one] [2=critically endangered] [3=pangolin] species.
					reportingSentence = "There {0} {6} {2} {3} subspecies. ";
				} else {
					// [5=family] contains [1=one] [2=endangered] species.
					// leave out includesClause when only subspecies
					//reportingSentence = "{5}{7} contains {6} {2} subspecies. "; 
					reportingSentence = "{5} contains {6} {2} subspecies. ";
				}
			}

			string firstSentences = string.Format(reportingSentence,
				(cr_count == 1 ? "is" : "are"),
				cr_count.NewspaperNumber(), // .UpperCaseFirstChar(),
				status_full,
				VernacularStringLower(),
				null, // "species",
				taxon,
				cr_infras_count.NewspaperNumber(),
				includesClause
				);

			//TODO
			string secondSentence = "";
			if (cr_pops_count > 0) {
				string also = "";
				if (cr_infras_count > 0 || cr_count > 0) {
					also = " also";
				}

				string popsText = node.StocksOrSubpopsText(cr_pops_count, true, status_full);

				// There [0=is/are] [1=also ][2=two endangered stocks/subpopulations].
				string secondSentenceTemplate = "There {0}{1} {2}. ";
				//string secondSentenceTemplate = "There {0}{1} been {0}{1} {2}. ";

				secondSentence = string.Format(secondSentenceTemplate,
					(cr_pops_count == 1 ? "is" : "are"),
					also,
					popsText
					//cr_pops_count.NewspaperNumber(), // .UpperCaseFirstChar(),
				);


			}

			return firstSentences + secondSentence;
		}

		public string PrintStatsBeforeBitris() {
			return null;
		}

	}
}

