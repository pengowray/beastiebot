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

            string[] cats = new string[] { "least concern", "near threatened", "vulnerable", "endangered", "critically endangered", "recently extinct" };
            foreach (string cat in cats) {
                footer.AppendLine("* [[List of " + cat + " " + node.nodeName.LowerPluralOrTaxon() + @"]]");
            }

            // TODO: other taxa with same threat level as current page

            // TODO: subcategories, e.g. endangered insects

            // TODO: broader cats: e.g. 

            footer.AppendLine();

            footer.AppendLine(@"== References ==");
            footer.AppendLine(@"{{Reflist}}");
            footer.AppendLine();
            footer.AppendLine(@"[[Category:IUCN Red List " + status.Text() + @" species|*" + uPlaxon + @"]]");

            return footer.ToString();
        }
            
    }
}
