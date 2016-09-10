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

        string _commonLower = null; // a lowercase version of the common name. Proper nouns are still capitalized (e.g. California). For now this only comes from the rules list. Value of "" means a cached null result.

        bool commonNameFromRules = false; // was the commonName taken from a rules file rather than the wiki? Note: should be lowercase if from rules.
        public bool commonNameFromIUCN { get; private set; } // was commonName taken from IUCN Red List common name list? (true only after calling CommonName() once)
        //bool pluralLoaded = false;

        TaxonNode node;  // IUCN node, really just for the rank info

        XowaPage redirFromPage;
        bool isRedir;
        List<IUCNBitri> otherBitrisLinkingHere; // may contain a value if DoesABetterBinomialLinkHere() returns true

        //string basicBitri; // bitri.BasicName(); // use "taxon" instead

        string taxoboxType = null;
        string taxoboxName = null; // name of template used
        public string taxonField = null; // the taxo name found in the taxobox
        string parentTaxonPageTitle = null; // for trinomials, what the title of the page the binomial redirects to

        //Note: use sp level for monotypic genus
        public enum Level { None, ssp, sp, genus, other };
        public Level pageLevel = Level.None;

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

            if (!string.IsNullOrEmpty(CommonNameLower())) {
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
        // done: lowercase common name from wiki (search wiki page for lowercase version)
        // done: "species in Mammalia" => "species in the class Mammalia"  (but only if "class" rank is identical in both Wiki and IUCN)
        // TODO?: "bat species" => "species of bat" ? meh.
        //
        // TODO: optionally link taxon (parameter: "link")
        //
        // Note: keep in sync with AdjectiveFormAvailable()
        //
        public override String Adjectivize(bool link = false, bool upperFirstChar = true, string noun = "species", string preposition = "within") {
            if (taxon == "top" || taxon == "Top") {
                // just return "species" for top level. (special case. could be a separate TaxonName subclass if needed.
                return noun.UpperCaseFirstChar(upperFirstChar);
            }

            // e.g. mammalian species
            if (rules != null && !string.IsNullOrEmpty(rules.adj)) {
                string adj = rules.adj.UpperCaseFirstChar(upperFirstChar);
                if (link) {
                    adj = MakeLink(originalPageTitle, adj);
                }
                return string.Format("{0} {1}", adj, noun);
            }

            // e.g. mammal species
            string common = upperFirstChar ? CommonName() : CommonNameLower();
            if (!string.IsNullOrEmpty(common)) {
                string adj = common.UpperCaseFirstChar(upperFirstChar); // CommonName() still may need uppercasing, e.g. if from rules list
                if (link) {
                    adj = MakeLink(originalPageTitle, adj);
                }
                return string.Format("{0} {1}", adj, noun); 
            }

            // e.g. species in the class Mammalia
            return string.Format("{0} {1} {2}", noun.UpperCaseFirstChar(upperFirstChar), preposition, TaxonWithRank(link));
        }

        //TODO
        // "4 species, 2 subspecies"    => "4 species and 2 subspecies in Mammalia"
        // or                           => "4 species and 2 subspecies of mammal"
        // "4 species"                  => "4 mammalian species"
        // desc="threatened", phrase="4 species, 2 subspecies" => "4 threatened species and 2 threatened subspecies in Mammalia"
        // IEnumerable<Tuple<string, string, int>> namePluralCount
        // output examples:
        // "There are 28 species and one variety in Primulales assessed as critically endangered"
        // "The IUCN also lists 140 plant subspecies and 10 varieties as critically endangered"
        // adjective form not used ("The IUCN also lists 14 mammalian subspecies and 20 mammalian subpopulations as critically endangered"
        // use: ToNewspaperSentence() instead
        public override String AdjectivizeMany(bool link = false, bool upperFirstChar = true, string preposition = "in", string phrase = "1 species, 2 subspecies, 3 varities", string desc = null) {
            string hphrase = phrase.Humanize(); // fix plurals hopefully.. 1 variety, 2 varieties
            string common = CommonNameLower();
            //TODO: desc (maybe ignore/remove)
            if (!string.IsNullOrEmpty(common)) {
                return hphrase + " of " + common;
            } else {
                return hphrase + " in " + TaxonWithRank(link);
            }
        }

        public override bool NonWeirdCommonName() {
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


        public override string TaxonWithRankDebug() {
            if (pageLevel == Level.sp)
                return taxon + " (species)";

            if (pageLevel == Level.ssp)
                return taxon + " (subspecies)";

            if (pageLevel == Level.genus) {
                if (bitri == null && node != null && node.rank == "genus") {
                    return taxon + " (genus)";
                }
            }

            return string.Format("{0} ({1})", taxon, node.rank);
        }

        // "the class Mammalia" or "Mammalia"
        public override string TaxonWithRank(bool link = false) {
            string ltaxon = taxon;
            if (link) {
                ltaxon = MakeLink(originalPageTitle, taxon);
            }

            if (pageLevel == Level.None)
                return ltaxon;

            if (pageLevel == Level.sp || pageLevel == Level.ssp)
                return "''" + ltaxon + "''"; // italicize
            

            if (pageLevel == Level.genus) {
                if (bitri == null && node != null && node.rank == "genus") {
                    return "the genus " + ltaxon; //TODO: italicize? (probably never used anyway)
                }

            } else if (node.isMajorRank()) {
                return "the " + node.rank + " " + ltaxon;
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
                    _commonLower = rules.commonName;
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
        [Obsolete]
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

        // output examples:
        // "[[Gorilla gorilla|Western gorilla]]"
        // "''[[Trachypithecus poliocephalus poliocephalus]]''" 
        // [[Cercopithecidae|Old World monkey]]
        override public string CommonNameLink(bool uppercase = true, PrettyStyle style = PrettyStyle.JustNames) {
            string common = CommonName();
            string wikilink = originalPageTitle;
            string taxonDisplay = taxon;
            string taxonBracketDisplay = "(" + taxon + ")";
            string taxonIsJustThisInItalics = null;

            if (bitri != null) {
                taxonDisplay = "''" + taxon + "''";
                taxonBracketDisplay = "''(" + taxon + ")''";
                taxonIsJustThisInItalics = taxon;
                string inrank = bitri.NormalizedInfrarank();
                if (!string.IsNullOrEmpty(inrank)) {
                    // e.g. ''Cycas szechuanensis'' subsp. ''fairylakea''
                    taxonDisplay = string.Format("''{0} {1}'' {2} ''{3}''", bitri.genus, bitri.epithet, inrank, bitri.infraspecies);
                    taxonBracketDisplay = string.Format("''({0} {1}'' {2} ''{3})''", bitri.genus, bitri.epithet, inrank, bitri.infraspecies);
                    taxonIsJustThisInItalics = null;

                } else if (taxon.Contains(" sp. nov.")) {
                    // e.g. "Acmella sp. nov. 'Ba Tai'"
                    // replace "sp. nov." with "sp." and don't italicize it
                    // TODO: should they part after "sp." be italicized?
                    taxonDisplay = "''" + taxon.Replace(" sp. nov.", "'' sp.");
                    taxonBracketDisplay = "(" + taxonDisplay + ")";
                    taxonIsJustThisInItalics = null;
                }
            }

            if (otherBitrisLinkingHere != null) {
                // e.g. Large Fig Parrot (3 LC birds)
                string orText = (wikilink == pageTitle ? "" : ", or to the page it redirects to ([[" + pageTitle + "]])");
                string note = " <!-- Note: Not linked to avoid confusion. Scientific names of more than one species on the IUCN Red List links to [[" + wikilink + "]]" + orText + ". List: " + otherBitrisLinkingHere.Select(bt => bt.BasicName()).JoinStrings(", ") + ". Please consider creating an article for this species or subspecies so it can be linked in future lists. -->"; 
                if (common == null) {
                    return taxonDisplay + note;

                } else {
                    return common.UpperCaseFirstChar() + " " + taxonBracketDisplay + note;
                }
            }

            if (common == null) {

                if (taxonIsJustThisInItalics == null) {
                    return MakeLink(wikilink, taxonDisplay, uppercase);
                } else {
                    return MakeItalicLink(wikilink, taxonIsJustThisInItalics, uppercase);
                }


            } else {
                if (style == PrettyStyle.NameAndSpecies) {
                    return MakeLink(wikilink, common, uppercase) + " " + taxonBracketDisplay;

                } else if (style == PrettyStyle.SpeciesAlwaysFirst) {
                    // for plants, keep taxonomic name first
                    //return MakeLink(wikilink, taxon, uppercase) + " (" + common.UpperCaseFirstChar() + ")";
                    if (taxonIsJustThisInItalics == null) {
                        return MakeLink(wikilink, taxonDisplay, uppercase) + ", " + common.UpperCaseFirstChar();
                    } else {
                        return MakeItalicLink(wikilink, taxonIsJustThisInItalics, uppercase);
                    }
                } else { 
                    return MakeLink(wikilink, common, uppercase);
                }
                
            }
        }

        //singular probably. probably uppercase anyway (unless a taxon given in lowercase, or found in rules)
        public string NameForText(bool upperFirstChar = false) {
            //TODO: don't italicize 'var.' etc
            string common = CommonName();
            if (common == null) {
                if (bitri != null) {
                    return "''" + taxon + "''";
                } else {
                    return taxon.UpperCaseFirstChar(upperFirstChar);
                }
            }

            return common.UpperCaseFirstChar(upperFirstChar); //TODO: uh.. we have proper lower and uppercase versions now?
        }

        // eg "[[Tarsiidae|Tarsier]] species" or  "[[Hominidae|Great apes]]" or "[[Lorisoidea]]"" or "[[Cetartiodactyla|Cetartiodactyls]]"
        override public string CommonNameGroupTitleLink(bool upperFirstChar = true, string groupof = "species") {
            string wikilink = originalPageTitle;

            string plural = Plural(upperFirstChar);
            if (plural != null) {
                return MakeLink(wikilink, plural, upperFirstChar);
            }

            string common = CommonNameLower();
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
        // todo: Delete link parameter. It's always == originalPageTitle.
        string MakeLink(string link, string display = null, bool uppercaseFirstChar = false) {
            if (display != null && uppercaseFirstChar)
                display = display.UpperCaseFirstChar();

            if (link == null) {
                link = originalPageTitle;
            }

            if (display == null || link == display) { // first character case is not important.
                return string.Format("[[{0}]]", link);
            } else if (link.UpperCaseFirstChar() == display.UpperCaseFirstChar()) {
                //TODO: only for Wikipedia, not Wiktionary
                return string.Format("[[{0}]]", link);
            }

            //TODO: if (display.EndsWith("s") && otherwise matches, make [[dog]]s link, or [[mammalia]]n etc

            return string.Format("[[{0}|{1}]]", link, display);
        }

        string MakeItalicLink(string link, string display = null, bool uppercaseFirstChar = false) {
            if (display != null && uppercaseFirstChar)
                display = display.UpperCaseFirstChar();

            if (link == null) {
                link = originalPageTitle;
            }

            if (display == null || link == display) { // first character case is not important.
                    return string.Format("''[[{0}]]''", link);
            } else if (link.UpperCaseFirstChar() == display.UpperCaseFirstChar()) {
                //TODO: only for Wikipedia, not Wiktionary
                return string.Format("''[[{0}]]''", link);
            }

            //TODO: if (display.EndsWith("s") && otherwise matches, make [[dog]]s link, or [[mammalia]]n etc

            return string.Format("''[[{0}|{1}]]''", link, display);

        }

        private string TryGeneratingCommonName(bool allowIUCNName = true) {
            if (!allowIUCNName)
                return null;

            if (taxon == null)
                return null;

            if (bitri == null) {
                //TODO: check that taxon node is an animal family 
                // ..or a plant subclass (i think they become -ids?)
                // also could be an animal superfamily (-oidae => -oid?)
                // find exceptions?
                if (taxon.EndsWith("idae")) {
                    _commonName = taxon.Substring(0, taxon.Length - "idae".Length).ToLowerInvariant() + "id"; // TODO: get last instance
                    _commonLower = _commonName;
                    return _commonName;
                }

                // TODO:
                // acea => aceans ?

            } else {

                string[] commonEngs = bitri.CommonNamesEng();

                if (commonEngs == null || commonEngs.Length == 0)
                    return null;

                foreach (string commonEng in commonEngs) {

                    bool ambiguous = IsCommonEngAmbiguous(commonEng);
                    if (!ambiguous) {
                        commonNameFromIUCN = true;

                        /*bool justAlwaysTitleCase = false;
                        if (justAlwaysTitleCase || char.IsUpper(commonEng[1])) {
                            // Second character is uppercase, so probably all uppercase. Change to title case.
                            // TODO: Sri lanka => Sri Lanka
                            // North american, etc
                            string titleCase = commonEng.ToLowerInvariant().UpperCaseFirstChar();
                            return titleCase;
                        }
                        */
                        string correctedCaps = RedListCapsReport.CorrectCaps(commonEng);

                        return correctedCaps;
                    }
                }
            }

            return null;
        }

        public bool IsCommonEngAmbiguous(string commonEng) {
            if (commonEng == null)
                return true;

            string lower = commonEng.ToLowerInvariant();

            // probably not needed any more
            string[] ambiguousNames = { "annual tropical killifish", "schmidly's deer mouse", "carp" };
            if (ambiguousNames.Contains(lower)) {
                // ambiguous name. 
                return true;
            }

            if (commonEng.StartsWith("species code")) {
                return true;
            }

            TaxaRuleList ruleList = TaxaRuleList.Instance();
            if (!bitri.isTrinomial) {

                if (ruleList.BinomAmbig != null && ruleList.BinomAmbig.Contains(commonEng.NormalizeForComparison())) {
                    // ambiguous... but... but nothing. ignore.
                    return true;
                }


            } else { // if (bitri.isTrinomial) 
                if (ruleList.BinomAmbig != null && ruleList.BinomAmbig.Contains(commonEng.NormalizeForComparison()))
                    return true; // ambiguous

                if (ruleList.InfraAmbig == null)
                    return true; // no subspecies ambig list in rules. So ignore: subspecies too likely to copy common name of species

                if (ruleList.InfraAmbig.Contains(commonEng.NormalizeForComparison()))
                    return true; // subspecies is ambiguous (used by another subspecies or a species)
            }

            if (commonEng != null) {
                if (commonEng.Length <= 2) {
                    return true; // 2 letter name? :/
                }
                if (pageTitle != null && commonEng.NormalizeForComparison() == pageTitle.NormalizeForComparison()) {
                    // already considered it. e.g. a better matching binomial links here.
                    return true;
                }

                return false;
            }

            return true;
        }

        override public string CommonName(bool allowIUCNName = true) {
            if (_commonName != null && allowIUCNName) {
                if (_commonName == string.Empty)
                    return null;

                return _commonName;
            }

            //quick, flawed check: if not a redirect then it's still the taxon name? 
            //fails if original page title was changed due to disambig in Rules
            //if (!isRedir || page == null) 
            //    return null;

            if (isTaxoboxBroaderNarrower()) {
                return TryGeneratingCommonName(allowIUCNName);
            }

            if (!HasTaxobox()) {
                return TryGeneratingCommonName(allowIUCNName );
            }

            if (isTitleTaxonomic()) { // redirect is to a scientific name still.
                return TryGeneratingCommonName(allowIUCNName );
            }

            if (pageTitle.StartsWith("Subspecies of ") || 
                    pageTitle.StartsWith("List of ") ||
                    pageTitle.StartsWith("Species of ")) {
                Console.Error.WriteLine("Note: '{0}' redirects to '{1}', which starts funny", taxon, pageTitle);
                return TryGeneratingCommonName(allowIUCNName );
            }

            if (DoesABetterBinomialLinkHere()) {
                return TryGeneratingCommonName(allowIUCNName);
            }

            string __commonName = pageTitle;
            // fix double space, such as in "Lipochromis sp. nov.  'backflash cryptodon'"
            __commonName = __commonName.Replace("  ", " ");

            if (__commonName.Contains(" (")) {
                // remove " (insect)" from "Cricket (insect)"
                __commonName = __commonName.Substring(0, __commonName.IndexOf(" ("));
            }

            if (allowIUCNName) {
                _commonName = __commonName;
            }

            return __commonName;
        }

        // if it returns true, check otherBitrisLinkingHere for the results.
        // returns false if it's the first one of the dupes
        public bool DoesABetterBinomialLinkHere() {
            if (bitri == null) // only for species
                return false;

            TaxaRuleList ruleList = TaxaRuleList.Instance();
            if (ruleList == null)
                return false;

            if (pageTitle == null) {
                return false; // whatever
            }

            string normalizedPageTitle = pageTitle.UpperCaseFirstChar();

            if (ruleList.WikiHigherDupes != null && ruleList.WikiHigherDupes.dupes.ContainsKey(normalizedPageTitle)) {
                // Not really a false synonym, but rather a link to a higher taxon, which should have been caught by isTaxoboxBroaderNarrower().
                // Anyway, we don't want to show it.
                otherBitrisLinkingHere = null;
                return true;
            }

            if (ruleList.WikiSpeciesDupes != null && ruleList.WikiSpeciesDupes.dupes.ContainsKey(normalizedPageTitle)) {
                var others = ruleList.WikiSpeciesDupes.allFoundNames[normalizedPageTitle];
                if (ruleList.WikiSpeciesDupes.allFoundNames[normalizedPageTitle][0] == bitri) {
                    // give a pass to the first dupe in the list
                    //Console.WriteLine("giving a pass to: " + pageTitle);
                    return false;
                } else {
                    //Console.WriteLine("multilink page: " + pageTitle);
                    otherBitrisLinkingHere = others;
                    return true;
                }
            }

            // Did not find any false synonyms for trinomials, so don't test for those.

            return false;
        }

        // common name with starting lowercase letter, unless it's a proper noun.
        // returns null if no common name, or if unsure
        public override string CommonNameLower() {
            if (_commonLower != null) {
                if (_commonLower == string.Empty)
                    return null;

                return _commonLower;
            }

            if (rules != null) {
                // get lowercase from rules

                _commonLower = rules.commonName;
                if (_commonLower != null) {
                    return _commonLower;
                }
            }

            string common = CommonName();
            if (common == null) {
                // no common name to build a lowercase from
                _commonLower = string.Empty;
                return null;
            }

            if (char.IsLower(common[0])) {
                _commonLower = common;
                return _commonLower;
            }

            string lowerCandidate = char.ToLowerInvariant(common[0]) + common.Substring(1);

            string upperRegex = @"\b" + common + @"\b";
            string lowerRegex = @"\b" + lowerCandidate + @"\b";

            int upperCount = Regex.Matches(page.text, upperRegex).Count;
            int lowerCount = Regex.Matches(page.text, lowerRegex).Count;

            int threshold = 2;
            int deltaThreshold = 2; // how many more lowers than uppers are needed to be sure

            if (lowerCount >= threshold  &&  lowerCount >= upperCount + deltaThreshold) {
                _commonLower = lowerCandidate;
                return _commonLower;

            } else if (upperCount >= threshold  &&  upperCount >= lowerCount + deltaThreshold) {
                _commonLower = common;
                return null;

            } else {

                _commonLower = string.Empty; // not sure.
                return null;
            }
        }

        bool pluralFromUpper = false;
        public override string Plural(bool okIfUppercase = false) { // ok if initial character is title case? if not then might return null instead
            if (_commonPlural != null) {
                if (_commonPlural == string.Empty)
                    return null;

                if (!okIfUppercase && pluralFromUpper)
                    return null;

                return _commonPlural;
            }


            if (rules != null) {
                // get plural from rules (which should never be titlecase)

                _commonPlural = rules.commonPlural;
                if (_commonPlural != null) {
                    return _commonPlural;
                }

                // shortcut: if it's a "frog" or "bat" in rules, then just add -s
                // note: never add a shortcut for "fish"
                
                string cn = rules.commonName;
                if (cn != null) {
                    if (cn.EndsWith(" frog") || cn.EndsWith(" bat") || cn.EndsWith(" worm") || cn.EndsWith(" spider") || cn.EndsWith(" lizard") || cn.EndsWith(" snake") || cn.EndsWith(" gecko")) {
                        _commonPlural = cn + "s";
                        return _commonPlural;
                    }
                }
            }

            // generate plural from common name and see if it's in the wiki text
            // e.g. 1. Microbat => Microbats
            // e.g. 2. Pupfish => null ("Pupfishes" not found on page)

            //string common = CommonName();
            string common = CommonNameLower();

            // -idae => -ids shortcut
            if (common != null && taxon.EndsWith("idae") && common.EndsWith("id")) {
                _commonPlural = common + "s";
                return _commonPlural;
            }

            if (common == null) {
                common = CommonName();
                pluralFromUpper = true;
            }

            if (common == null) {
                // no common name to build a plural from
                _commonPlural = string.Empty;
                pluralFromUpper = false;
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

                string regex = @"\b" + c + @"\b";
                //string regex = c;
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
                if (!okIfUppercase && pluralFromUpper)
                    return null;
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

            if (taxonEndings.Any(suffix => pageTitle.EndsWith(suffix)))
                return true;

            if (pageTitle.Contains(" sp. ")) // catches titles such as "Haplochromis sp. 'backflash cryptodon'" (which was weirdly renamed from "Lipochromis sp. nov. 'backflash cryptodon'", but anyway)
                return true;

            return false;
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

            var taxonFieldWords = taxonField.Split(' ');
            if (taxonFieldWords != null && taxonFieldWords.Length > 0) {
                //TODO: check for name field
                //TODO: remove "pageTitle == taxon" check from IsScientifical()
                if (taxonFieldWords.All(part => pageTitle.Contains(part))) {
                    return true; // all the bits are there
                }

                if (taxonFieldWords.First() == pageTitle) {
                    // possibly a genus monotypic genus
                    // e.g. Bermudagidiella bermudiensis has the title Bermudagidiella
                    // unfortunately this may give a false positive for some names: 
                    // e.g. Hippopotamus (Hippopotamus amphibius), Indri (Indri indri), caracal (Caracal caracal)
                    return true;
                }
            }

            //TODO: also check if page title is in italics (This is usually done by the taxobox template though)
            //https://en.wikipedia.org/wiki/Template:Taxobox#Italic_page_titles

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
                    cnError = string.Format("Note: page title '{1}' is binomial, not trinomial ({0}). Too broad so won't use for common name", bitri.FullDebugName(), pageTitle);
                    return true;
                }
            }

            LoadTaxobox();

            if (bitri != null && pageLevel == Level.genus) {
                cnError = string.Format("Note: '{0}' redirects to its genus '{1}', so not using as common name.", bitri.FullDebugName(), pageTitle);
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
                    cnError = string.Format("Note: '{0}' redirects to the same page as the binomial '{1}', so not used as common name.", bitri.FullDebugName(), pageTitle);
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
