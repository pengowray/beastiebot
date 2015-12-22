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
        public string commonNamePlural;
        bool weirdCommonName = false; // commonName or override contains the word "family", "fishes" or "species" or is plural
		bool notAssigned; // the name of this taxon is literally "Not assigned"

        TaxonPage taxonPage;
        //public string wikiName; // name on wikipedia, e.g. "Cricket (insect)"
		//TODO: wiki name override?

		//public string wikiedName; // [[Pholidota|Pangolin]] species
		//public string wikiedHeader; // "==[[Pholidota|Pangolin]] species=="

        public string comprises;
        public string includes;
        public string means;

        

        //public string reportingSentence;

        public TaxonHeader(TaxonNode node, string taxon, int depth, string comprises = null, string includes = null, string means = null) {
            this.node = node;
            this.taxon = taxon;
            this.depth = depth;

            this.comprises = comprises;
            this.includes = includes;
            this.means = means;

            //TODO: preserve common name / plural / etc and use as needed

            if (taxon == "Not assigned" || taxon == "ZZZZZ Not assigned") {
                //wikiedName = "Not assigned";
                notAssigned = true;
                //reportingSentence = "This group";
                //reportingSentence = "This group contains {1} {2} {3}";

            } else {
                taxonPage = BeastieBot.Instance().GetTaxonPage(taxon);
                //this.wikiName = taxonPage.originalPageTitle;

            }
        }

        string WikiHeading() { 
            if (notAssigned) {
                return "Not assigned";
            } else {
                return taxonPage.CommonNameGroupTitleLink();
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
			string line = string.Format("{0}{1}{0}", tabs, WikiHeading(), tabs);

			return line;
		}

        public string GrayText() {
            if (!string.IsNullOrWhiteSpace(means)) {
                return "{{gray|(\"" + means.UpperCaseFirstChar() + "\")}}"; // {{gray|("means")}} // note: string.Format turns {{ into {.
            }

            if (!string.IsNullOrWhiteSpace(comprises)) {
                //return string.Format(@"{{gray|{0}}}", comprises);
                return "{{gray|" + comprises.UpperCaseFirstChar() + "}}"; // {{gray|comprises}}

            }

            if (!string.IsNullOrWhiteSpace(includes)) {
                return "{{gray|Includes " + includes + "}}"; // {{gray|Includes includes}}
            }

            return null;
        }

        public string VernacularStringLower() {
            if (notAssigned) {
                return "\"not assigned\""; // lit: "not assigned" (with quotes)
            }

            return taxonPage.CommonOrTaxoNameLowerPref();
        }

		public string PrintStatsBeforeSplit(RedStatus status) {
			// {0}=are/is, {1}=number, {2}=status/adjective (e.g. "critically endangered"), {3}=species/subspecies/subpopulation(s)
			// {4}=commonName (lowercased), {5}=taxon
			// "There are 1000 critically endangered pangolin species" => "There {0} {1} {2} pangolin {3}" 
			// "There are 1000 critically endangered pangolin species, and 500 critically endangered subspecies." => "There {0} {1} {2} pangolin {3}" 
			// "Myomorpha contains 1,137 critically endangered species" => "Myomorpha contains {1} {2} {3}"
			// This group contains 1,233 critically endangered species and 23 critically endangered subspecies."
			// This group contains 1,233 critically endangered species, 23 critically endangered subspecies. There are also 4 endangered subpopulations."

			//reportingSentence = "Within the {4} there {0} {2} {3}"; // for weird one containing the text "family"

			if (notAssigned)
				return null; // don't bother with special stats for "Not assigned" taxon.

            if (status.Limited() == RedStatus.None) {
                return null;
            }

            //Dictionary<string, string> codes_en = new Dictionary<string, string>();
            //codes_en["CR"] = "critically endangered";

            int all_count = node.DeepBitriCountWhere(b => !b.isStockpop && !b.isTrinomial); // all assessed (including DD)
			int cr_count = node.DeepBiCount(status);
			int cr_infras_count = node.DeepBitriCountWhere(b => !b.isStockpop && b.isTrinomial && b.Status == status);
			int cr_pops_count = node.DeepBitriCountWhere(b => b.isStockpop && b.Status == status);

            string status_full = status.Text(); //codes_en[status];

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
            
            return GrayText();
        }

    }
}

