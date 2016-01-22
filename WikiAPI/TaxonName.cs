﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    // supertype for other TaxonName classes and the one to use for genus, species or subspecies which don't have a wikipedia page

    // usually use subclass TaxonPage

    public class TaxonName { //was: TaxonPageSp

        public string taxon { get; protected set; }
        public bool isAssigned = true;

        public TaxonRules rules { get; protected set; } // stays null except for in subclass TaxonPage

        public TaxonName(string taxon) {
            this.taxon = taxon;
        }

        virtual public string CommonOrTaxoNameLowerPref() {
            return "''" + taxon + "''";
        }

        virtual public string CommonNameLink(bool uppercase = true) {
            return string.Format("''[[{0}]]''", taxon);
        }

        // "group of" parameter may be ignored if a plural common name is found, or common name is a collective noun
        // returns eg "[[Tarsiidae|Tarsier]] species" or  "[[Hominidae|Great apes]]" or "[[Lorisoidea]]"" or "[[Cetartiodactyla|Cetartiodactyls]]"
        virtual public string CommonNameGroupTitleLink(bool upperFirstChar = true, string groupof = "species") {
            //ignore groupof for species
            return CommonNameLink(upperFirstChar);
        }

        virtual public string CommonName() {
            return null;
        }

        // Note: keep in sync with Adjectivize()
        public virtual bool AdjectiveFormAvailable() {
            return false;
        }

        // e.g. Adjectivize("subpopulations", "of") might return "subpopulations of Balaenoptera musculus"
        public virtual String Adjectivize(bool link = false, bool upperFirstChar = true, string noun = "species", string preposition = "within") {
            //TODO: [[link]] if link is true
            if (upperFirstChar) {
                return string.Format("{0} {1} {2}", noun.UpperCaseFirstChar(), preposition, taxon);
            } else {
                return string.Format("{0} {1} {2}", noun, preposition, taxon);
            }
        }

        // "the class Mammalia" or "Mammalia"
        public virtual string TaxonWithRank() {
            return "''" + taxon + "''";
        }
    }
}