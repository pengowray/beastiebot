using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    class BlurbFooter {

        public static string Footer(TaxonNode node, RedStatus status) {


            StringBuilder footer = new StringBuilder();

            // [[List of endangered mammals]]
            // [[Category:IUCN Red List critically endangered species|*Mammals]]"

            if (node.name == "Mammalia") {
                // "recently extinct";
            }

            string uPlaxon = node.nodeName.UpperPluralOrTaxon();
            string lPlaxon = node.nodeName.LowerPluralOrTaxon();

            // See also list: 
            footer.AppendLine(@"== See also ==");
            // TODO: recently extinct only exists for birds and mammmals currently

            string listsPage = status.ListsPage(); // e.g. "Lists of IUCN Red List critically endangered species"
            if (!string.IsNullOrEmpty(listsPage)) {
                footer.AppendLine(@"* [[" + listsPage + @"]]");
            }

            //string[] cats = new string[] { "least concern", "near threatened", "vulnerable", "endangered", "critically endangered", "recently extinct", "data deficient" };
            RedStatus[] pageCats = new RedStatus[] { RedStatus.LC, RedStatus.NT, RedStatus.VU, RedStatus.EN, RedStatus.CR, RedStatus.EX, RedStatus.DD };
            foreach (var cat in pageCats) {
                if (cat == status)
                    continue;

                if (status == RedStatus.EXplus && cat == RedStatus.EX)
                    continue;

                footer.AppendLine("* [[" + node.nodeName.ListOf(cat) + @"]]"); // e.g. List of near threatened mammals
            }

            if (node.IsOrParentIs("Fish")) {
                footer.AppendLine("* [[Sustainable seafood advisory lists and certification]]");
            }


            // TODO: more see also, e.g.

            // * [[Conservation of fungi]]

            // TODO: other taxa with same threat level as current page

            // TODO: subcategories, e.g. endangered insects

            footer.AppendLine();

            if (node.HasNotesMatching(status)) {
                footer.AppendLine(@"== Notes ==");
                //footer.AppendLine(@"{{Notelist}}");
                footer.AppendLine(@"{{notelist-ua}}");
                footer.AppendLine();
            }

            footer.AppendLine(@"== References ==");
            footer.AppendLine(@"{{Reflist}}");
            footer.AppendLine();
            string thisList = null;
            if (status.isNull()) {
                thisList = node.nodeName.LowerPluralOrTaxon().UpperCaseFirstChar() + " by conservation status"; // e.g. Fungi by conservation status
            } else {
                thisList = status.TextWithRecently().UpperCaseFirstChar() + " " + node.nodeName.LowerPluralOrTaxon(); // e.g. Endangered bats
                footer.AppendLine(@"[[Category:IUCN Red List " + status.Text() + @" species|*" + uPlaxon + @"]]");
            }

            bool invertFound = false;
            if (node.IsOrParentIs("Animalia")) {
                footer.AppendLine("[[Category:Lists of animals by conservation status|" + thisList + "]]");
                if (node.IsOrParentIs("CHORDATA")) { //TODO: work out if all IUCN CHORDATA are vertebrates
                    if (node.IsOrParentIs("Aves")) {
                        footer.AppendLine("[[Category:Lists of birds|" + thisList + "]]");
                        if (status.isThreatened() || status == RedStatus.NT) {
                            footer.AppendLine("[[Category:Bird conservation]]");
                        }
                    } else if (node.IsOrParentIs("Fish")) { // added taxon
                        footer.AppendLine("[[Category:Lists of fishes|" + thisList + "]]");
                        footer.AppendLine("[[Category:Fish by conservation status|" + thisList + "]]");
                        if (status == RedStatus.EN) {
                            // [[Category:Endangered fish|]] // TODO: rename "threatened fishes"
                            footer.AppendLine("[[Category:" + status.Text().UpperCaseFirstChar() + " fish|*" + thisList + "]]");
                        }
                    } else if (node.IsOrParentIs("Mammalia")) {
                        footer.AppendLine("[[Category:Lists of mammals|" + thisList + "]]");
                        if (status.isThreatened() || status == RedStatus.NT) {
                            footer.AppendLine("[[Category:Mammal conservation]]");
                        }
                    } else if (node.IsOrParentIs("REPTILIA")) {
                        footer.AppendLine("[[Category:Lists of reptiles|" + thisList + "]]");
                        if (status.isThreatened() || status == RedStatus.NT) {
                            footer.AppendLine("[[Category:Reptile conservation]]");
                        }

                    } else if (node.IsOrParentIs("AMPHIBIA")) {
                        footer.AppendLine("[[Category:Lists of amphibians|" + thisList + "]]");
                        // note: currently no Category:Amphibians conservation‎

                    } else {
                        //note: all chordata in red list are vertebrate
                        footer.AppendLine("[[Category:Lists of vertebrates|" + thisList + "]]");
                    }
                } else {
                    invertFound = true;
                    footer.AppendLine("[[Category:Lists of invertebrates|" + thisList + "]]");
                }
            } else if (node.IsOrParentIs("PLANTAE")) {
                // note: does not currently exist. Add to: [[Category:Lists of plants]]
                footer.AppendLine("[[Category:Lists of plants by conservation status|" + thisList + "]]");
                if (status == RedStatus.None) {
                    // blank

                } else if (status == RedStatus.EW) {
                    footer.AppendLine("[[Category:Plants extinct in the wild| ]]");
                } else { 
                    footer.AppendLine("[[Category:" + status.Text().UpperCaseFirstChar() + " plants| ]]");
                }
            } else if (node.IsOrParentIs("FUNGI")) {
                //footer.AppendLine("[[Category:Lists of fungi|" + thisList + "]]");
                footer.AppendLine("[[Category:Lists of fungi|Conservation status]]");
                footer.AppendLine("[[Category:Lists of biota by conservation status|Fungi]]");
            }

            if (!invertFound && node.IsOrParentIs("Invertebrate")) { // PseduoNode 'Invertebrate' might not be listed under Animalia
                footer.AppendLine("[[Category:Lists of invertebrates|" + thisList + "]]");
            }

            if (node.IsOrParentIs("INSECTA")) {
                footer.AppendLine("[[Category:Lists of insects|" + thisList + "]]");
                if (!status.isNull()) {
                    footer.AppendLine("[[Category:Insects by conservation status|" + thisList + "]]");
                    if (status == RedStatus.CR) {
                        footer.AppendLine("[[Category:Critically endangered insects|*" + thisList + "]]");
                    }

                }
            } else if (node.IsOrParentIs("ARACHNIDA")) {
                footer.AppendLine("[[Category:Lists of arachnids|" + thisList + "]]");

            } else if (node.IsOrParentIs("Arthropoda")) {
                footer.AppendLine("[[Category:Lists of arthropods|" + thisList + "]]");
                if (status.isThreatened() || status == RedStatus.NT) {
                    footer.AppendLine("[[Category:Arthropod conservation]]");
                }
            } else if (node.IsOrParentIs("Chromista")) {
                footer.AppendLine("[[Category:Lists of biota by conservation status|Chromista]]");
                footer.AppendLine("[[Category:Eukaryotes]]"); //? (added to article by User:KConWiki)
                footer.AppendLine("[[Category:Protista]]");
            }

            footer.AppendLine();
            footer.AppendLine("{{bots|deny=BG19bot,Yobot}}"); // avoid pointless and sometimes deleterious bot edits. If not strong enough, try:
            //footer.AppendLine("{{bots|deny=all}}"); // "Other bots besides BG19bot and non-AWB bots do the same thing" says Bgwhite, so may need to deny all.

            return footer.ToString();
        }
            

        public static string Cats(TaxonNode node, RedStatus status) {
            return null;
        }
    }
}
