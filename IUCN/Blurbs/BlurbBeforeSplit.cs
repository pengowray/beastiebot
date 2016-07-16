using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class BlurbBeforeSplit : Blurb {
        //was: PrintStatsBeforeSplit(RedStatus status)
        public static string Text(TaxonNode node, RedStatus status, int depth, bool includeGray = true) {
            if (depth == 0) {
                return null; // use ArticleBlurb() instead
            }

            if (status.Limited() == RedStatus.None) {
                return null;
            }

            //Dictionary<string, string> codes_en = new Dictionary<string, string>();
            //codes_en["CR"] = "critically endangered";

            TaxonStats allStats = node.GetStats();
            TaxonStats statusStats;
            if (status == RedStatus.EXplus) {
                statusStats = node.GetStats(RedStatus.EX);
            } else {
                statusStats = node.GetStats(status);
            }

            int sp_count = allStats.species; //node.DeepSpeciesCount(); // all assessed (including DD)
            int cr_count = statusStats.species; //node.DeepSpeciesCount(status);
            int cr_subsp = statusStats.subspecies; // included varieties
            int cr_vars = statusStats.subspecies_varieties;
            int cr_actual_subsp = statusStats.subspecies_actual_subsp;

            int cr_pops_sp = statusStats.subpops_species;
            int cr_pops_ssp = statusStats.subpops_subspecies;
            int cr_pops_total = statusStats.subpops_total;

            if (status == RedStatus.EXplus) {
                cr_pops_sp = 0;
                cr_pops_ssp = 0;
                cr_pops_total = 0;
            }

            /*
            TaxonName nodeName = node.nodeName;
            bool useCommon = (!string.IsNullOrEmpty(commonName) && (commonName != taxon) && !weirdCommonName);
            string reportingSentence = "";

            string includesClause = string.Empty;
            string includesSentence = string.Empty;
            if (!string.IsNullOrWhiteSpace(includes)) {
                includesClause = ", which includes " + includes + ",";
                includesSentence = "Includes " + includes + ". "; // not currently used
            }
    */
            // {0}=are/is, {1}=number, {2}=status/adjective (e.g. "critically endangered"), {3}=species/subspecies/subpopulation(s)
            // {4}=commonName (lowercased), {5}=taxon
            // "There are 1000 critically endangered pangolin species" => "There {0} {1} {2} pangolin {3}" 
            // "There are 1000 critically endangered pangolin species, and 500 critically endangered subspecies." => "There {0} {1} {2} pangolin {3}" 
            // "Myomorpha contains 1,137 critically endangered species" => "Myomorpha contains {1} {2} {3}"
            // This group contains 1,233 critically endangered species and 23 critically endangered subspecies."
            // This group contains 1,233 critically endangered species, 23 critically endangered subspecies. There are also 4 endangered subpopulations."

            //reportingSentence = "Within the {4} there {0} {2} {3}"; // for weird one containing the text "family"

            string isare = (cr_count == 1 ? "is" : "are"); // {0}
            string isareSsp = (cr_subsp == 1 ? "is" : "are");
            string number = cr_count.NewspaperNumber(); // {1} // .UpperCaseFirstChar(),
            string status_full = status.Text(); // {2}
            // null, // "species", {3}
            //string vern_lower = VernacularStringLower(); // {4}
            //string taxon, {5}
            string numberSsp = cr_subsp.NewspaperNumber(); // {6}
                                                           //string includesClause

            string subspWord = "subspecies";
            if (cr_vars > 0) {
                if (cr_actual_subsp > 0) {
                    //TODO: list subspecies and varieties separately
                    subspWord = "subspecies and varieties";
                } else {
                    subspWord = "varieties"; // TODO: fix "one varieties"
                }

            }

            string nounPhrase = node.nodeName.Adjectivize(false, false, "species", "in"); // "mammalian species" or "species in the class Mammalia"
            string nounPhraseSsp = node.nodeName.Adjectivize(false, false, subspWord, "in"); // "mammalian subspecies" or "subspecies in the class Mammalia"

            string firstSentences = "";

            if (cr_count > 0 && cr_subsp == 0) {
                //string sentence = string.Format("There are {0} {2} {1} listed.",
                firstSentences = string.Format("There are {0} {1} assessed as {2}. ",
                    number,
                    nounPhrase,
                    status_full);

                // old way when !AdjectiveFormAvailable() :
                // [5=family] contains [1=one] [2=endangered] species.
                //reportingSentence = "{5}{7} contains {1} {2} species. ";

                // old way when AdjectiveFormAvailable() :
                // There [0=is] [1=one] [2=critically endangered] [3=pangolin] species.
                //reportingSentence = includesSentence + "There {0} {1} {2} {3} species. ";

            } else if (cr_count > 0 && cr_subsp > 0) {


                if (node.nodeName.AdjectiveFormAvailable()) {
                    // ... [6=six] (ssps.) [2=endangered]
                    //reportingSentence = "There {0} {1} {2} {3} species and {6} {2} subspecies. ";

                    firstSentences = string.Format("There {0} {1} {2} and {3} {4} assessed as {5}. ",
                        isare,
                        number,
                        nounPhrase,
                        numberSsp,
                        nounPhraseSsp,
                        status_full
                        );

                } else {
                    //reportingSentence = "{5}{7} contains {1} {2} species and {6} {2} subspecies. ";
                    firstSentences = string.Format("There {0} {1} species and {2} {5} in {3} assessed as {4}. ",
                        isare,
                        number,
                        numberSsp,
                        node.nodeName.TaxonWithRank(),
                        status_full,
                        subspWord
                        );

                }
            } else if (cr_count == 0 && cr_subsp > 0) {
                firstSentences = string.Format("There {0} {1} {2} assessed as {3}. ",
                    isareSsp,
                    number,
                    nounPhraseSsp,
                    status_full);
            }

            string secondSentence = "";
            if (cr_pops_total > 0) {
                string also = "";
                if (cr_subsp > 0 || cr_count > 0) {
                    also = " also";
                }

                // "There are also subpopulations of mammalian species and subspecies assessed as critically endangered"
                // "There are also subpopulations of species and subspecies in Mammalia assessed as critically endangered"
                // "There is also a subpopulation of ..."

                //string popsText = node.StocksOrSubpopsText(cr_pops_count, true, status_full);

                string isare_subpop = "are";
                string subpopulations = "subpopulations";
                if (cr_pops_total == 1) {
                    isare_subpop = "is";
                    subpopulations = "a subpopulation";
                }

                string speciesAndSubpecies = "";
                if (cr_pops_sp > 0 && cr_pops_ssp > 0) {
                    speciesAndSubpecies = "species and subspecies"; // note: there are no subpopulations of varieties
                } else if (cr_pops_sp > 0) {
                    speciesAndSubpecies = "species";
                } else {
                    speciesAndSubpecies = "subspecies";
                }

                string nounPhraseSubpop = node.nodeName.Adjectivize(false, false, speciesAndSubpecies, "in");

                // There [0=are][1= also] [2=subpopulations] of [3=mammalian species and subspecies] assessed as [4=endangered].
                secondSentence = string.Format("There {0}{1} {2} of {3} assessed as {4}. ",
                    isare_subpop,
                    also,
                    subpopulations,
                    nounPhraseSubpop,
                    status_full
                    );


                //cr_pops_count.NewspaperNumber(), // .UpperCaseFirstChar(),
            }
            string twoSentences = firstSentences + secondSentence;

            if (status == RedStatus.EXplus) {
                TaxonStats PE = node.GetStats(RedStatus.PE);
                TaxonStats EW = node.GetStats(RedStatus.EW);
                TaxonStats PEW = node.GetStats(RedStatus.PEW);

                if (PE.species > 0 || PE.subspecies > 0)
                    twoSentences += Text(node, RedStatus.PE, depth, false);

                if (EW.species > 0 || EW.subspecies > 0)
                    twoSentences += Text(node, RedStatus.EW, depth, false);

                if (PEW.species > 0 || PEW.subspecies > 0)
                    twoSentences += Text(node, RedStatus.PEW, depth, false);
            }

            string greySentence = "";
            if (includeGray) {
                greySentence = GraySentence(node);
            }
            //TODO: Taxon contains / includes / comprises sentence.

            return greySentence + twoSentences;
        }

        public static string GraySentence(TaxonNode node) {
            var rules = node.rules;

            if (rules == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(rules.comprises)) {
                //return string.Format(@"{{gray|{0}}}", comprises);
                return node.nodeName.taxon + " comprises " + rules.comprises + ". ";

            }

            if (!string.IsNullOrWhiteSpace(rules.includes)) {
                //return string.Format(@"{{gray|{0}}}", comprises);
                return node.nodeName.taxon + " includes " + rules.includes + ". ";
            }

            return string.Empty;
        }


    }
}
