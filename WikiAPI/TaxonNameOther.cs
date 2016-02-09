using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace beastie {
    class TaxonNameOther : TaxonName {
        TaxonName parent;
        public bool allowTitlecase = false;

        public TaxonNameOther(TaxonName parent) : base("Other " + parent.taxon) {
            this.parent = parent;
            this.isAssigned = false; // i guess?
        }

        override public string CommonOrTaxoNameLowerPref() {
            return "other " + parent.LowerPluralOrTaxon();
        }

        override public string CommonNameLink(bool upperFirstChar = true) {
            // don't show links for "other" taxa

            return "other ".UpperCaseFirstChar(upperFirstChar) + parent.LowerPluralOrTaxon();
        }


        override public string CommonNameGroupTitleLink(bool upperFirstChar = true, string groupof = "species") {
            if (groupof != null) {
                return "other ".UpperCaseFirstChar(upperFirstChar) + parent.LowerOrTaxon(allowTitlecase) + " " + groupof;
            } else {
                return "other ".UpperCaseFirstChar(upperFirstChar) + parent.LowerPluralOrTaxon();
            }
        }

        override public string CommonName(bool allowIUCNName = true) {
            return "Other " + parent.LowerPluralOrTaxon();
        }

        public override string CommonNameLower() {
            return "other " + parent.LowerPluralOrTaxon();
        }

        public override bool AdjectiveFormAvailable() {
            return false;
        }

        //TODO: but shouldn't be needed
        //public override String Adjectivize(bool link = false, bool upperFirstChar = true, string noun = "species", string preposition = "within") {

    }

}