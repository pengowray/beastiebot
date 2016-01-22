using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    class BlurbFooter {

        public static string Footer(TaxonNode node, RedStatus status) {

            // [[List of endangered mammals]]
            // [[Category:IUCN Red List critically endangered species|*Mammals]]"

            string footerText =
@"== See also == 
* [[List of endangered " + node.nodeName.LowerPluralOrTaxon() + @"]]

== References ==
{{Reflist}}

[[Category:IUCN Red List " + status.Text() + @" species|*" + node.nodeName.UpperPluralOrTaxon() + @"]]";

            return footerText;
        }
    }
}
