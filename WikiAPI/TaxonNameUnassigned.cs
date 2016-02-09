using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class TaxonNameUnassigned : TaxonName {

        public TaxonNameUnassigned(string taxon) : base(taxon) {
            isAssigned = false;
        }

        override public string CommonOrTaxoNameLowerPref() {
            return "not assigned";
        }

        override public string CommonNameLink(bool uppercase = true) {
            if (uppercase) {
                return "Not assigned";
            } else {
                return "not assigned";
            }
        }

        override public string CommonNameGroupTitleLink(bool upperFirstChar = true, string groupof = "species") {
            return CommonNameLink(upperFirstChar);
        }

        override public string CommonName(bool allowIUCNName = true) {
            return null;
        }
    }
}
