using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace beastie {
	public class TaxonHeaderBlurb : Blurb //TODO: split into TaxonStatusStats, Blurb classes, and TaxonNodeName
    {

        // e.g. "==[[Mammalia|Mammals]]==" or "==[[Mammalia]] species=="
        // "groupof" may or may not appear in the returned results
		public static string HeadingString(TaxonNode node, int depth, RedStatus status) {
			bool noHeader0 = true; // no header for depth 0 (e.g. "Mammal species")
			bool skipDepth1 = true; // don't return "=Pangolin species=", instead "==Pangolin species=="

            if (noHeader0 && depth == 0) {
				return string.Empty;
			}


            string groupof = null;
            var stats = node.GetStats(status);
            if (stats.species > 0 && stats.subspecies == 0 && stats.subpops_total == 0) {
                // show "species" in heading only if it's not going to be in a subheading
                groupof = "species";
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
            string heading = node.nodeName.CommonNameGroupTitleLink(true, groupof);

            string line = string.Format("{0}{1}{0}", tabs, heading, tabs);

			return line;
		}

        //TODO: move to other static class?
        public static string GrayText(TaxonNode node) {
            var rules = node.rules;

            if (rules == null)
                return null;

            if (!string.IsNullOrWhiteSpace(rules.means)) {
                return "{{gray|(\"" + rules.means.UpperCaseFirstChar() + "\")}}"; // {{gray|("means")}} // note: string.Format turns {{ into {.
            }

            if (!string.IsNullOrWhiteSpace(rules.comprises)) {
                //return string.Format(@"{{gray|{0}}}", comprises);
                return "{{gray|" + rules.comprises.UpperCaseFirstChar() + "}}"; // {{gray|comprises}}

            }

            if (!string.IsNullOrWhiteSpace(rules.includes)) {
                return "{{gray|Includes " + rules.includes + "}}"; // {{gray|Includes includes}}
            }

            return null;
        }

        /*
        // replaced with TaxonName.CommonOrTaxoNameLowerPref()
        public string VernacularStringLower() {
            if (notAssigned) {
                return "\"not assigned\""; // lit: "not assigned" (with quotes)
            }

            return taxonPage.CommonOrTaxoNameLowerPref();
        }
        */


        public static string ArticleBlurb(TaxonNode node, RedStatus status) {
            StringBuilder blurb = new StringBuilder();
            var cr_stats = node.GetStats(status);
            var all_stats = node.GetStats();

            int cr_count = cr_stats.species; //node.DeepSpeciesCount(status);
            int sp_count = all_stats.species; //ode.DeepSpeciesCount();

            blurb.AppendFormat("As of {0}, the [[International Union for Conservation of Nature]] (IUCN) lists {1} {2}{3}",
                FileConfig.Instance().iucnRedListFileDate, // {0} date
                cr_count, // {1} species count
                (status == RedStatus.Null ? "" : "[[" + status.Text() + "]] " ), // {2} status with link (optional)
                //(taxon == "top" ? "" : taxon + " ") // {3} taxon group // TODO: adjective form (e.g. "mammalian")
                node.nodeName.Adjectivize(true, false, "species") // {3} taxon species group name ("mammalian species" or "species within Mammalia")
                );

            if (status == RedStatus.CR) {
                int pe_count = node.GetStats(RedStatus.PE).species; //node.DeepSpeciesCount(RedStatus.PE);
                int pew_count = node.GetStats(RedStatus.PEW).species; //node.DeepSpeciesCount(RedStatus.PEW);
                int combined_count = pe_count + pew_count;

                if (combined_count > 0) {
                    if (pe_count > 0 && pew_count == 0) {
                        blurb.AppendFormat(", including {0} which are tagged as ''possibly extinct''.",
                            pe_count
                       );
                    } else {
                        blurb.AppendFormat(", including {0} which are tagged as ''possibly extinct'' or ''possibly extinct in the wild''.",
                            combined_count
                       );

                    }

                } else {
                    blurb.Append(", none of which are tagged as ''possibly extinct''.");
                }
                blurb.Append(FileConfig.Instance().iucnRedListFileRef);
                blurb.Append(FileConfig.Instance().iucnPossiblyExtinctFileRef);
                blurb.Append(" ");

            } else {
                blurb.Append(".");
                blurb.Append(FileConfig.Instance().iucnRedListFileRef);
                blurb.Append(" ");
            }

            if (status != RedStatus.Null && cr_count > 0) {
                blurb.AppendFormat("{0} of all evaluated {1} are listed as {2}. ",
                    Percent(cr_count, sp_count),
                    //was: "{1}species", (taxon == "top" ? "" : taxon + " "),
                    node.nodeName.Adjectivize(false, false, "species"),
                    status.Text()
                    );

                // debug info:
                blurb.AppendFormat("<!-- {0} / {1} -->", cr_count, sp_count);
            }

            blurb.AppendLine();
            blurb.AppendLine();

            //TODO next
            //"The IUCN also lists 59 mammalian subspecies and 17 mammalian stocks or populations as critically endangered."

            blurb.Append(@"{{TOC limit|2}}");

            return blurb.ToString();
        }

        static string Percent(int count, int total) {
            double percent = (double)count / (double)total;
            if (percent > .1f) { // e.g. 11%
                return percent.ToString("P0");
            } else if (percent > .01f) {  // e.g. 1.1%
                return percent.ToString("P1");
            } else {  // e.g. 0.11%
                return percent.ToString("P2");
            }
        }

        

    }
}

