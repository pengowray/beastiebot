using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DotNetWikiBot;
using System.Text.RegularExpressions;
using Humanizer.Inflections;
using Humanizer;

namespace beastie {
    public class TaxonPage : TaxonName { //was: BitriPage

        // this can be a page for either a bitri, or a taxon. If it's a bitri, then taxon = bitri.BasicName().
        IUCNBitri bitri;
        //string taxon; // in base class

        BeastieBot _beastieBot;
        BeastieBot beastieBot {
            set { _beastieBot = value; }
            get {
                if (_beastieBot == null)
                    return BeastieBot.Instance();

                return _beastieBot;
            }
        }


        XowaPage page; // the main page (may be where it redirects to)
        public string originalPageTitle; // before redirect (exists regardless of if the page redirects). Also the wikilink. May be influenced by rules.wikilink[taxon] to produce e.g. "Anura (frog)" not "Anura" (disambig)
        public string pageTitle; 
        string _commonName = null; // tidied version of pageTitle if pageTitle is a common name. Cached result of CommonName(). Value of "" means a cached null result.
        string _commonPlural = null; // use CommonPlural(). plural or group name, e.g. "lemurs" (to be used in place of "Lemuroidea species"). Value of "" means a cached null result.

        string lowercaseCommonName = null; // a lowercase version of the common name. Proper nouns are still capitalized (e.g. California). For now this only comes from the rules list.
        bool commonNameFromRules = false; // was the commonName taken from a rules file rather than the wiki? Note: should be lowercase if from rules.
        //bool pluralLoaded = false;

        TaxonNode node;  // IUCN node, really just for the rank info

        XowaPage redirFromPage;
        bool isRedir;

        //string basicBitri; // bitri.BasicName(); // use "taxon" instead

        string taxoboxType = null;
        string taxoboxName = null; // name of template used
        string taxonField = null; // the taxo name found in the taxobox
        string parentTaxonPageTitle = null; // for trinomials, what the title of the page the binomial redirects to

        //Note: use sp level for monotypic genus
        public enum Level { None, ssp, sp, genus, other };
        Level pageLevel = Level.None;

        //bool taxonFieldIsGenus;
        //bool taxonFieldIsBi;
        //bool taxonFieldIsTri;

        public string cnError; // the error encountered when trying to find the common name

        // if it ends with one of these, it's probably not a common name (but need an exception list?)
        private string[] taxonEndings = new string[] { "idae", "aceae", "inae", "iformes", "oidei", "ii",
        "phyta","phytina", "opsida", "idae", "phycota", "phycotina", "phyceae", "phycidae", "mycota", "mycotina", "mycetes", "mycetidae",
        "anae", "ales", "ineae", "aria", "acea", "oidea", "oidae", "aceae", "idae", "oideae", "inae", "odd", "ini",
        "inae", "ptera", "ozoa", "icha", "oela"}; //excluded: iti, ad, ina, ia, ura, pha,

        public TaxonPage(TaxonNode node, string taxon, BeastieBot beastieBot = null) : base (taxon) {
            this.beastieBot = beastieBot;
            this.node = node; // note: don't use node.name because it just links to this.taxon

            Load();
        }

        public TaxonPage(string taxon, BeastieBot beastieBot = null) : base(taxon) {
            this.beastieBot = beastieBot;
            //this.taxon = taxon;

            Load();
        }

        public TaxonPage(IUCNBitri bitri, BeastieBot beastieBot = null) : base(bitri.BasicName()) {
            this.beastieBot = beastieBot;
            this.bitri = bitri;
            //this.taxon = bitri.BasicName(); // e.g. "Lariscus insignis" or "Tarsius bancanus natunensis" (no "ssp." etc)

            Load();
        }

        // Note: keep in sync with Adjectivize()
        public override bool AdjectiveFormAvailable() {
            if (rules != null && !string.IsNullOrEmpty(rules.adj)) {
                return true;
            }

            if (!string.IsNullOrEmpty(lowercaseCommonName)) {
                return true;
            }

            return false;
        }

        // returns: 
        // * "mammalian species", (using adj if available)
        // * "mammal species" (using common name as adjective)
        // * "species within Mammalia" (best attempt with taxon and provided preposition)
        //
        // other e.g's: "blue whale subpopulations"
        //
        // doesn't return: "subpopulations of blue whales"
        //
        // only uses adj or common name from rules
        // preposition examples: in, of, within -- may or may not end up in the output
        // TODO: lowercase common name from wiki (search wiki page for lowercase version)
        // TODO: "species in Mammalia" => "species in the class Mammalia"  (but only if "class" rank is identical in both Wiki and IUCN)
        // TODO?: "bat species" => "species of bat" ? meh.
        // todo: "4 species and 2 subspecies" => "4 mammalian species and 2 mammalian subspecies" OR "4 species and 2 subspecies in Mammalia"
        // TOOD: optionally link taxon
        //
        // Note: keep in sync with AdjectiveFormAvailable()
        //
        public override String Adjectivize(bool link = false, bool upperFirstChar = true, string noun = "species", string preposition = "within") {
            if (rules != null && !string.IsNullOrEmpty(rules.adj)) {
                return string.Format("{0} {1}", rules.adj, noun);
            }

            if (!string.IsNullOrEmpty(lowercaseCommonName)) { // lowercaseCommonName from rules
                return string.Format("{0} {1}", lowercaseCommonName, noun);
            }

            return string.Format("{0} {1} {2}", noun, preposition, TaxonWithRank());
        }

        public bool NonWeirdCommonName() {
            string common = CommonName();
            if (string.IsNullOrEmpty(common))
                return false; // doesn't even have one

            if (common.Contains("species"))
                return false;

            if (common.Contains("family"))
                return false;
            
            if (common.Contains("fishes"))
                return false;

            return true;
        }

        // "the class Mammalia" or "Mammalia"
        public override string TaxonWithRank() {
            if (pageLevel == Level.None)
                return taxon;
            
            if (pageLevel == Level.sp || pageLevel == Level.ssp)
                return "''" + taxon + "''"; // italicize

            if (pageLevel == Level.genus) {
                if (bitri == null && node != null && node.rank == "genus") {
                    return "the genus " + taxon; //TODO: italicize? (probably never used anyway)
                }

            } else if (node.isMajorRank()) {
                return "the " + node.rank + " " + taxon;
            }
            

            return taxon;
        }

        void Load() {
            //TODO: make rules optional / configurable
            TaxaRuleList ruleList = TaxaRuleList.Instance();
            rules = ruleList.GetDetails(taxon);

            if (rules != null) {
                if (!string.IsNullOrEmpty(rules.commonName)) {
                    commonNameFromRules = true;
                    _commonName = rules.commonName;
                    lowercaseCommonName = rules.commonName;
                }

                originalPageTitle = rules.wikilink;
            }

            if (originalPageTitle == null) { 
                originalPageTitle = taxon; // may be rewriten again below
            }

            XowaPage firstPage = beastieBot.GetPage(originalPageTitle, false); // xowa.ReadXowaPage(basicName);

            if (firstPage != null) {
                originalPageTitle = firstPage.title;
            } else {
                return;
            }

            isRedir = (firstPage != null && !string.IsNullOrEmpty(firstPage.text) && firstPage.IsRedirect());

            if (isRedir) {
                string rto = firstPage.RedirectsTo();

                if (!string.IsNullOrEmpty(rto)) {
                    page = beastieBot.GetPage(rto, false);
                }
                redirFromPage = firstPage;

            } else {
                page = firstPage;
            }

            if (page != null) {
                pageTitle = page.title;
            }

            //LoadTaxobox();  // wait til it's needed

            //Plural();
        }


        //public bool ArticleCoversBitri() {
        //    return !isTaxoboxBroaderNarrower();
        //}

        public int ArticleLength() {
            return page.text.Length; // todo: remove templates, references, and other junk
        }


        // aka: VernacularStringLower()
        // lower case common name preferably, otherwise correctly capitalized taxon. italics on binomials etc.
        override public string CommonOrTaxoNameLowerPref() {
            string common = CommonName();

            //if (notAssigned) {
                //return "\"not assigned\""; // lit: "not assigned" (with quotes)
            //}

            if (common == null) {
                if (bitri != null) {
                    //TODO: italics on genus too

                    return "''" + taxon + "''";
                } else {
                    return taxon.UpperCaseFirstChar();
                }
            }

            //TODO: don't lowercase "American" etc
            //TODO: maybe only lowercase first character?

            return common.ToLowerInvariant();
        }

        // eg "[[Gorilla gorilla|Western gorilla]]" or "''[[Trachypithecus poliocephalus poliocephalus]]''" or [[Cercopithecidae|Old World monkey]]
        override public string CommonNameLink(bool uppercase = true) {
            string common = CommonName();
            string wikilink = originalPageTitle;

            if (common == null) {
                return MakeLink(wikilink, taxon, uppercase);

            } else {
                return MakeLink(wikilink, common, uppercase);
            }
        }

        //singular probably. probably uppercase anyway (unless a taxon given in lowercase, or found in rules)
        public string NameForText(bool upperFirstChar = false) {
            string common = CommonName();
            if (common == null) {
                if (bitri != null) {
                    return "''" + taxon + "''";
                } else {
                    return (upperFirstChar ? taxon.UpperCaseFirstChar() : taxon);
                }
            }

            return (upperFirstChar ? common.UpperCaseFirstChar() : common);
        }


        // eg "[[Tarsiidae|Tarsier]] species" or  "[[Hominidae|Great apes]]" or "[[Lorisoidea]]"" or "[[Cetartiodactyla|Cetartiodactyls]]"
        override public string CommonNameGroupTitleLink(bool upperFirstChar = true, string groupof = "species") {
            string wikilink = originalPageTitle;

            string plural = Plural();
            if (plural != null) {
                return MakeLink(wikilink, _commonPlural, upperFirstChar);
            }

            string common = CommonName();
            if (common != null) {
                if (bitri != null || !NonWeirdCommonName() || string.IsNullOrEmpty(groupof) ) {

                    return MakeLink(wikilink, common, upperFirstChar);
                } else {

                    return MakeLink(wikilink, common, upperFirstChar) + " " + groupof; // + " species";
                }

            } else {

                return MakeLink(wikilink, taxon, upperFirstChar);
            }
        }

        public string CommonNameGroupNoLink(bool uppercase = true) {
            // todo: return same as CommonNameGroupTitleLink but without the link.
            return null;
        }

        // remove redundant display parameter if it's not needed, adds italics if bitri
        string MakeLink(string link, string display = null, bool uppercaseFirstChar = false) {
            if (display != null && uppercaseFirstChar)
                display = display.UpperCaseFirstChar();

            if (display == null || link == display ) {
                if (bitri != null) {
                    return string.Format("''[[{0}]]''", link);
                } else {
                    return string.Format("[[{0}]]", link);
                }
            }

            //TODO: if (display.EndsWith("s") && otherwise matches, make [[dog]]s link
            //TODO: if link.UpperCaseFirstChar() == display.UpperCaseFirstChar() then don't split them, but only for Wikipedia, not Wiktionary

            //if (bitri != null) {
            //    return string.Format("''[[{0}|{1}]]''", link, display); // mistake: display is common name, so shouldn't be italic.
            //} else {
            //    return string.Format("[[{0}|{1}]]", link, display);
            //}

            return string.Format("[[{0}|{1}]]", link, display);
        }

        override public string CommonName() {
            if (_commonName != null) {
                if (_commonName == string.Empty)
                    return null;

                return _commonName;
            }

            //quick, flawed check: if not a redirect then it's still the taxon name? 
            //fails if original page title was changed due to disambig in Rules
            //if (!isRedir || page == null) 
            //    return null;

            if (isTaxoboxBroaderNarrower())
                return null;

            if (!HasTaxobox()) {
                return null;
            }

            if (isTitleTaxonomic()) // redirect is to a scientific name still.
                return null;

            if (pageTitle.StartsWith("Subspecies of ") || 
                    pageTitle.StartsWith("List of ") ||
                    pageTitle.StartsWith("Species of ")) {
                Console.Error.WriteLine("Note: '{0}' redirects to '{1}', which starts funny", taxon, pageTitle);
                return null;
            }

            _commonName = pageTitle;
            // fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
            _commonName = _commonName.Replace("  ", " ");

            if (_commonName.Contains(" (")) {
                // remove " (insect)" from "Cricket (insect)"
                _commonName = _commonName.Substring(0, _commonName.IndexOf(" ("));
            }

            return _commonName;
        }

        public string Plural() {
            if (_commonPlural != null) {
                if (_commonPlural == string.Empty)
                    return null;

                return _commonPlural;
            }


            if (rules != null) {
                // get plural from rules

                _commonPlural = rules.commonPlural;
                if (_commonPlural != null) {
                    return _commonPlural;
                }
            }


            // generate plural from common name and see if it's in the wiki text
            // e.g. 1. Microbat => Microbats
            // e.g. 2. Pupfish => null ("Pupfishes" not found on page)

            string common = CommonName();

            if (common == null) {
                // no common name to build a plural from
                _commonPlural = string.Empty;
                return null;
            }

            // try finding candidate pluralizations on the page

            Dictionary<string, int> candidates = new Dictionary<string, int>(); // candidate plural term, count of appearances on the wiki page
            candidates[common + "s"] = 0;
            candidates[common + "es"] = 0;
            candidates[common.Pluralize()] = 0;

            int highest = 0;
            string best = null;

            foreach (var c in candidates.Keys) {
                if (c == common) {
                    // give a warning?
                    continue;
                }

                //string regex = @"\b" + c + @"\b";
                string regex = c;
                int count = Regex.Matches(page.text, regex, RegexOptions.IgnoreCase).Count;
                //candidates[c] = count; // for debugging, so can show all candidates

                if (count > highest) {
                    best = c;
                    highest = count;
                }
            }

            int threshold = 2; // minimum number of times it must appear to count

            if (highest >= threshold) {
                _commonPlural = best;
                return _commonPlural;

            } else {
                _commonPlural = string.Empty;
                return null;
            }
        }

        public bool HasTaxobox() {
            LoadTaxobox();
            if (taxoboxType == null || taxoboxType == "none")
                return false;

            return true;
        }

        // Does the pageTitle end with something like -aceae (used in scientific names). Does
        // Use isTitleTaxonomic() to also check taxobox
        public bool LooksScientifical() {
            // assumes pageTitle exists.

            //flawed: may have changed due to disambig in Rules
            //if (!isRedir)
            //    return true; // too easy?

            if (pageTitle == taxon)
                return true;

            return (taxonEndings.Any(suffix => pageTitle.EndsWith(suffix)));
        }

        // defaults to false if unsure (e.g. redirect page doesn't have a taxobox)
        public bool isTitleTaxonomic() {
            // assumes pageTitle and taxonField exist.

            // quick check: looks scientifical
            if (LooksScientifical()) {
                //TODO: option to ignore
                //TODO: check scientific name database too?
                cnError = string.Format("Redirects to '{0}', which looks like it's another scenitific name, so not using for common name", pageTitle);
                return true;
            }

            LoadTaxobox();

            if (string.IsNullOrWhiteSpace(taxonField)) {
                //cnError = string.Format("No taxobox or taxon field found for {0}", taxon);
                return false; // can't tell.. TODO: do other tests?
            }

            if (pageTitle.Contains(taxonField)) {
                return true;
            }

            if (taxonField.Split(' ').All(part => pageTitle.Contains(part))) {
                return true; // all the bits are there
            }

            return false;
        }

        // does the page redirect to a broader (or narrower) page than the taxon we're looking for.
        // note: mainly for species and subspecies level taxa. If bitri is null, might not give very accurate results
        public bool isTaxoboxBroaderNarrower() {
            if (!isRedir && !string.IsNullOrEmpty(pageTitle)) {
                return false; // the page title is the taxon
            }

            // quick check before loading taxobox: genus title for bi/tri
            if (bitri != null && pageTitle == bitri.genus) {
                //TODO: though it is sometimes also the common name when monotypic (Puda pudu? or something)
                cnError = string.Format("Redirects to its genus '{0}', so not using as common name.", pageTitle);
                return true;
            }

            // quick check before loading taxobox: trinomial redirects to binomial
            if (bitri != null && bitri.isTrinomial) {
                if (pageTitle == bitri.ShortBinomial()) {
                    cnError = string.Format("Note: page title '{1}' is binomial, not trinomial ({0}), too broad so can't use for common name", bitri.FullName(), pageTitle);
                    return true;
                }
            }

            LoadTaxobox();

            if (bitri != null && pageLevel == Level.genus) {
                cnError = string.Format("Note: '{0}' redirects to its genus '{1}', so not using as common name.", bitri.FullName(), pageTitle);
                return true;
            }

            if (bitri != null && bitri.isTrinomial) {
                if (pageLevel != Level.ssp) {
                    return true;
                }

                //TODO: move to "private void LoadBinomPageTitle()"
                if (parentTaxonPageTitle == null) {
                    parentTaxonPageTitle = beastieBot.PageNameInWiki(bitri.ShortBinomial());
                    if (parentTaxonPageTitle == null)
                        parentTaxonPageTitle = string.Empty; // empty means we've checked for it previously
                }

                if (!string.IsNullOrEmpty(parentTaxonPageTitle) && parentTaxonPageTitle == pageTitle) {
                    // trinomial redirects to same page as binomial.
                    // Assume there are no subsp which are synonymous with their species
                    cnError = string.Format("Note: '{0}' redirects to the same page as the binomial '{1}', so not used as common name.", bitri.FullName(), pageTitle);
                    return true;
                }

                return false;

            } else if (bitri != null && !bitri.isTrinomial) {

                if (pageLevel != Level.sp)
                    return true;

                return false;
            }

            // we don't really check very hard when looking at a non bi/tri
            return false;
        }


        void LoadTaxobox() {
            if (page == null)
                return;

            if (taxoboxType != null)
                return;

            Page tpage = page.ToPage();

            taxoboxType = "taxobox";
            taxoboxName = FindTemplateName(tpage, "Taxobox");

            if (taxoboxName == null) {
                taxoboxType = "auto";
                taxoboxName = FindTemplateName(tpage, "Automatic taxobox");
            }

            if (taxoboxName == null) {
                taxoboxType = "auto";
                taxoboxName = FindTemplateName(tpage, "Automatic Taxobox"); // a redirect
            }

            if (taxoboxName == null) {
                taxoboxType = "speciesbox";
                taxoboxName = FindTemplateName(tpage, "Speciesbox");
            }

            if (taxoboxName == null) {
                taxoboxType = "none";
                cnError = "No taxobox-like templates found for: " + taxon + " => " + pageTitle;
                return;
            }

            if (taxoboxType == "none") {
                return;

            } else if (taxoboxType == "speciesbox") {
                taxonField = tpage.GetFirstTemplateParameter(taxoboxName, "taxon");
                if (taxonField == null) {
                    //cnError = basicBitri + "=>" + pageTitle + " - empty taxon field";
                    //return null;
                } else {
                    taxonField = taxonField.Trim();
                }


                string genusField = tpage.GetFirstTemplateParameter(taxoboxName, "genus");
                string speciesField = tpage.GetFirstTemplateParameter(taxoboxName, "species");

                if (string.IsNullOrEmpty(taxonField)) {
                    if (string.IsNullOrEmpty(genusField)) {
                        if (string.IsNullOrEmpty(speciesField)) {
                            cnError = "Speciesbox has no taxon, genus, or species: " + taxon + " => " + pageTitle;
                            return;
                        } else {
                            cnError = "(Wiki needs fixing) Speciesbox has a species field but no genus (or taxon): " + taxon + " => " + pageTitle;
                            taxonField = speciesField.Trim();
                            pageLevel = Level.sp;  // kinda
                        }
                    } else {
                        if (string.IsNullOrEmpty(speciesField)) {
                            cnError = "Speciesbox has a genus field but no taxon or species: " + taxon + " => " + pageTitle;
                            taxonField = genusField.Trim();
                            pageLevel = Level.genus;
                        } else {
                            taxonField = speciesField.Trim() + " " + genusField.Trim();
                            pageLevel = Level.sp;
                        }
                    }

                } else { // taxon field not empty
                    if (string.IsNullOrEmpty(genusField)) {
                        if (string.IsNullOrEmpty(speciesField)) {
                            // ok, just use taxon field 
                            pageLevel = Level.sp;

                        } else {
                            cnError = "(Wiki needs fixing) Speciesbox has species field but no genus. Using taxon field. " + taxon + " => " + pageTitle;
                            // whatever, use taxon
                            pageLevel = Level.sp;  // probably
                        }
                    } else {
                        if (string.IsNullOrEmpty(speciesField)) {
                            // genus and no species, use taxon
                            
                            if (taxonField.Contains(" ")) {
                                cnError = "Speciesbox has genus field but no species. Probably monotypic (it's speciesbox, not genusbox). Using taxon field. " + taxon + " => " + pageTitle;
                                pageLevel = Level.sp;  // probably
                                //TODO: look closer at taxon field

                            } else {
                                cnError = "Speciesbox has genus field but no species. And taxon field is one word. Guess it's a genus? (bad template usage)" + taxon + " => " + pageTitle;
                                pageLevel = Level.genus;
                            }

                        } else {

                            string taxonField2 = speciesField.Trim() + " " + genusField.Trim();
                            if (taxonField.Contains(taxonField2)) {
                                // they match. cool.
                                pageLevel = Level.sp;

                            } else {

                                cnError = "Speciesbox has both taxon field and species+genus. But they don't seem to match: " + taxonField +  " vs " + taxonField2 + ". " + taxon + " => " + pageTitle;
                                // but whatever. TODO: separate warnings and errors?
                                //TODO: store other taxon name? (taxonField2)
                                pageLevel = Level.sp;
                            }
                        }
                    }
                }

            } else if (taxoboxType == "taxobox" || taxoboxType == "auto") {
                string genus = tpage.GetFirstTemplateParameter(taxoboxName, "genus");
                string binomial = tpage.GetFirstTemplateParameter(taxoboxName, "binomial");
                string trinomial = tpage.GetFirstTemplateParameter(taxoboxName, "trinomial");

                if (!string.IsNullOrWhiteSpace(trinomial)) {
                    taxonField = trinomial;
                    pageLevel = Level.ssp;
                } else if (!string.IsNullOrWhiteSpace(binomial)) {
                    taxonField = binomial;
                    pageLevel = Level.sp;
                } else if (!string.IsNullOrWhiteSpace(genus)) {
                    taxonField = genus;
                    pageLevel = Level.genus;
                } else {
                    //cnError = "No binomial, trinomial or genus in taxobox. Probably a higher level. " + taxon + " => " + pageTitle;
                    pageLevel = Level.other;
                }

                if (taxonField != null) {
                    //taxonField = taxonField.Replace("'", "");
                    //TODO: remove templates, references
                    //remove daggers, quotes and other shit
                    taxonField = Regex.Replace(taxonField, "[^a-zA-Z0-9% ._]", string.Empty); //TODO: refine
                    taxonField = taxonField.Replace("  ", " "); // normalize spaces a bit
                    taxonField = taxonField.Trim();
                }
            }

        }

        public static string FindTemplateName(Page page, string templateName) {
            string wanted = templateName.Trim().NormalizeSpaces().UpperCaseFirstChar();
            foreach (var t in page.GetTemplates(false, false)) {
                // remove comments, trim and get case right
                string result = Regex.Replace(t, "<!--.*?-->", "", RegexOptions.Singleline).Trim().NormalizeSpaces().UpperCaseFirstChar();
                if (result == wanted)
                    return t;
            }

            return null;
        }
    }
}
