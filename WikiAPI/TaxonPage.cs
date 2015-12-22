using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DotNetWikiBot;
using System.Text.RegularExpressions;

namespace beastie {
    public class TaxonPage { //was: BitriPage

        // this can be a page for either a bitri, or a taxon. If it's a bitri, then taxon = bitri.BasicName().
        IUCNBitri bitri;
        string taxon;

        BeastieBot beastieBot;

        XowaPage page; // the main page (may be where it redirects to)
        public string originalPageTitle; // before redirect (exists regardless of if the page redirects). Also the wikilink. May be influenced by rules.wikilink[taxon] to produce e.g. "Anura (frog)" not "Anura" (disambig)
        public string pageTitle; 
        string commonName = null; // tidied version of pageTitle if pageTitle is a common name. Cached result of CommonName(). Value of "" means a cached null result.
        bool commonNameFromRules = false; // was the commonName taken from a rules file rather than the wiki? Note: should be lowercase if from rules.
        string plural = null; // plural or group name, e.g. "lemurs" (to be used in place of "Lemuroidea species"). Always from rules (currently)

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

        public TaxonPage(BeastieBot beastieBot, string taxon) {
            this.beastieBot = beastieBot;
            this.taxon = taxon;

            Load();
        }

        public TaxonPage(BeastieBot beastieBot, IUCNBitri bitri) {
            this.beastieBot = beastieBot;
            this.bitri = bitri;
            this.taxon = bitri.BasicName(); // e.g. "Lariscus insignis" or "Tarsius bancanus natunensis" (no "ssp." etc)

            Load();
        }

        void Load() {
            //TODO: make rules optional / configurable
            TaxonDisplayRules rules = TaxonDisplayRules.Instance();

            rules.taxonCommonPlural.TryGetValue(taxon, out plural);
            rules.taxonCommonName.TryGetValue(taxon, out commonName);
            if (!string.IsNullOrEmpty(commonName)) {
                commonNameFromRules = true;
            }

            rules.wikilink.TryGetValue(taxon, out originalPageTitle); // possibly disambig the starting page title via rules
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
        }

        //public bool ArticleCoversBitri() {
        //    return !isTaxoboxBroaderNarrower();
        //}

        public int ArticleLength() {
            return page.text.Length; // todo: remove templates, references, and other junk
        }


        // aka: VernacularStringLower()
        // lower case common name preferably, otherwise correctly capitalized taxon. italics on binomials etc.
        public string CommonOrTaxoNameLowerPref() {
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
        public string CommonNameLink(bool uppercase = true) {
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
        public string CommonNameGroupTitleLink(bool upperFirstChar = true) {
            string wikilink = originalPageTitle;

            if (plural != null) {
                return MakeLink(wikilink, plural, upperFirstChar);
            }

            string common = CommonName();
            if (common != null) {
                if (bitri != null || common.Contains("species") || common.Contains("family") || common.Contains(" fishes")) {

                    return MakeLink(wikilink, common, upperFirstChar);
                } else {

                    return MakeLink(wikilink, common, upperFirstChar) + " species";
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

        public string CommonName() {
            if (commonName != null) {
                if (commonName == string.Empty)
                    return null;

                return commonName;
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

            commonName = pageTitle;
            // fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
            commonName = commonName.Replace("  ", " ");

            if (commonName.Contains(" (")) {
                // remove " (insect)" from "Cricket (insect)"
                commonName = commonName.Substring(0, commonName.IndexOf(" ("));
            }

            return commonName;
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
