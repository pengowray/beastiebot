using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//delete me

namespace beastie {
    // extract out details of TaxonNode here?

    // currently just handles English

    public class TaxonNodeName {
        public string taxon; // scientific name. was: "name"
        public TaxonNode node;

        //TaxonPage taxonPage;
        TaxonName taxonPage;

        public string commonName;
        public string commonNameOverride;
        public string commonNamePlural;

        bool useCommon;
        bool weirdCommonName = false; // commonName or override contains the word "family", "fishes" or "species" or is plural


        public bool isAssigned {
            get {
                return (!string.IsNullOrEmpty(taxon)) && taxon != "Not assigned" && taxon != "ZZZZZ Not assigned";
            }
        }

        public TaxonNodeName(string name, TaxonNode node) {
            this.taxon = name;
            this.node = node;

            if (isAssigned) {
                taxonPage = BeastieBot.Instance().GetTaxonPage(name);
                //this.wikiName = taxonPage.originalPageTitle;
            }

            useCommon = (!string.IsNullOrEmpty(commonName) && (commonName != taxon) && !weirdCommonName);
        }

        string WikiHeading() {
            if (isAssigned) {
                return taxonPage.CommonNameGroupTitleLink();
            } else {
                return "Not assigned";
            }
        }

        




    }
}
