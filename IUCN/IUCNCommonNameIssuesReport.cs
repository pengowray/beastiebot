using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace beastie {
    public class IUCNCommonNameIssuesReport {
        TaxonNode topNode;
        TextWriter output;

        public IUCNCommonNameIssuesReport(TaxonNode topNode, TextWriter output) {
            this.topNode = topNode;
            this.output = output;
        }

        public void MakeReport() {
            output.WriteLine("https://en.wikipedia.org/wiki/User:Beastie_Bot/IUCN_common_name_issues");
            output.WriteLine("A list of possible common name errors or issues of names found in the IUCN Red List. IUCN data downloaded " + FileConfig.Instance().iucnRedListFileDate);
            WeirdJoiners();
            Dot();
            DoubleSpace();
            OddApostrophe();
            QuestionMark();
            Symbols();
            Numbers();
            KnownSpelling();
            KnownPlurals();
            The();
            FB();
            SpeciesCode();
            AllCaps();
            OddCaps();
            PossiblePlurals();
            SpNov();
            SymbolsInScientificName();
        }


        public void WeirdJoiners() {
            output.WriteLine("==Separators==");
            bool issueFound = false;
            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField != null && namesField.Contains(" - ")) {
                    output.WriteLine("* ''" + bitri.FullName() + "'' (" + namesField + "): Common Names field contains dash with spaces, ' - '. Perhaps should be a comma (,) or remove spaces.");
                    issueFound = true;
                } else if (namesField != null && namesField.Contains("--")) {
                    output.WriteLine("* ''" + bitri.FullName() + "'' (" + namesField + "): Common Names field contains double dash, '--'. Perhaps should be a comma (,)");
                    issueFound = true;
                }
            }

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField != null && namesField.Contains(";")) {
                    output.WriteLine("* ''" + bitri.FullName() + "'' (" + namesField + "): Common Names field contains semicolon (;). Perhaps should be a comma (,)");
                    issueFound = true;
                }
            }

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField != null && namesField.Contains(" or ")) {
                    output.WriteLine("* ''" + bitri.FullName() + "'' (" + namesField + "): Common Names field contains 'or'. Perhaps should be a comma (,)");
                    issueFound = true;
                }
            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void The() {
            output.WriteLine("==Redundant ''the''==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase)) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name begins with 'the' (Probably redundant)");
                        issueFound = true;
                    }
                }

            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }


        public void DoubleSpace() {
            output.WriteLine("==Double space==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.Contains("  ")) {
                        string showspaces = name.Replace(" ", "&nbsp;");
                        output.WriteLine("* " + bitri.FullName() + " (" + showspaces + "): Common name contains double space");
                        issueFound = true;
                    }
                }

            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void QuestionMark() {
            output.WriteLine("==Question marks==");
            output.WriteLine("Common name contains one or more question marks. Possibly due to Unicode characters which cannot be written to the non-Unicode CSV file.");
            output.WriteLine();
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.Contains("?")) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + ")");
                        issueFound = true;
                    }
                }

            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void KnownSpelling() {
            output.WriteLine("==Known spelling errors==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                //string[] errors = { "africansspeckled", "tropiacl", "sspined", "mout" };
                // if (Regex.IsMatch(name, @"\b(tropiacl|sspined|mout)\b", RegexOptions.IgnoreCase)) {

                foreach (string name in names) {
                    string lower = name.ToLowerInvariant();

                    if (Regex.IsMatch(name, @"\b(mout)\b", RegexOptions.IgnoreCase)) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name word contains possible spelling error (mout vs mount).");
                        issueFound = true;
                    } else if (Regex.IsMatch(name, @"\b(tropiacl)\b", RegexOptions.IgnoreCase)) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name word contains known spelling error (tropiacl).");
                        issueFound = true;
                    } else if (Regex.IsMatch(name, @"\b(ss)", RegexOptions.IgnoreCase)) {
                        // e.g. Cobitis puncticulata = brown spined loach // listed as 'Brown Sspined Loach'
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name word starts with a double s (possible error).");
                        issueFound = true;
                    } else if (lower.Contains((bitri.genus + "eng").ToLowerInvariant()) || lower.Contains((bitri.epithet + "eng").ToLowerInvariant())) {
                        // Sphenomorphus decipiens = black-sided sphenomorphus // listed as 'Black-sided Sphenomorphuseng'
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name word is scientific name + 'eng' (possible error).");
                        issueFound = true;
                    } else if (Regex.IsMatch(name, @"\BSs\B")) { // \B = non-word boundry
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name contains 'Ss' in the middle (possible error).");
                        issueFound = true;

                    }

                }


            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void Numbers() {
            output.WriteLine("==Numbers==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.Any(char.IsNumber)) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name contains numbers (possible error)");
                        issueFound = true;
                    }
                }

            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void Symbols() {
            output.WriteLine("==Symbols==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    // e.g. Main´s nipple-cactus
                    var oddSymbols = @"!@#$%^&*_+[]/\|~:{}".ToCharArray(); // semicolon (;) already listed in Separators. weird accent (´) elsehwere too
                    bool match = name.IndexOfAny(oddSymbols) != -1;
                    bool alreadyCovered = name.ToLowerInvariant().Contains("species code");

                    if (match && !alreadyCovered) { 
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name contains symbol(s)");
                        issueFound = true;
                    }
                }

            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void OddApostrophe() {
            output.WriteLine("==Apostrophe==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.IndexOf('´') != -1) {
                        // e.g. Main´s nipple-cactus
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + ") — common name contains acute accent (´), possibly used as apostrophe (')");
                        issueFound = true;
                    } else if (name.IndexOfAny("´’‛ˈ".ToCharArray()) != -1) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + ") — common name contains possible strange apostrophe");
                        issueFound = true;
                    }

                }
            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void Dot() {
            output.WriteLine("==Dot==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.EndsWith(".")) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + ") — Common name ends with a dot");
                        issueFound = true;
                    }
                }

            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }




        public void FB() {
            output.WriteLine("==FB==");
            output.WriteLine("Names ending in '(fb)'");
            output.WriteLine();
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());
                foreach (string name in names) {
                    var lower = name.ToLowerInvariant();
                    if (lower.EndsWith("(fb)")) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' — " + name);
                        issueFound = true;
                    }
                }

            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void SpeciesCode() {
            output.WriteLine("==Species code==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());
                foreach (string name in names) {
                    var lower = name.ToLowerInvariant();
                    if (lower.Contains("species code")) {
                        output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): appears to be a species code rather than a common name");
                        issueFound = true;
                    }
                }

            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        HashSet<string> knownPluralNames = new HashSet<string>();

        public void KnownPlurals() {
            output.WriteLine("==Likely plurals==");
            bool issueFound = false;

            // ok: texas
            // but: king of the mullets, Crown Of Thorns, Baby's Tears
            // uncertain: drummers gobbleguts jumbos grass-eaters paperbones barreleyes aurochs spiderlegs pepperpants saddlebags Turkey-peas grains dreams

            string[] knownPlurals = "toads frogs crabs bats anchovies cats snails mullets snappers razorback tetras silversides herrings badgers snakes treefrogs fishes wrasses rats carps".Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            knownPlurals = knownPlurals.Distinct().OrderBy(a => a).ToArray();
            output.WriteLine("Names ending with: " + knownPlurals.JoinStrings(", "));
            output.WriteLine();

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());
                foreach (string name in names) {
                    var lower = name.ToLowerInvariant();

                    if (lower.EndsWith("s")) {
                        if (lower.Contains(" of the ")) continue; // (King Of The Mullets, King Of The Breams
                        if (knownPlurals.Any(pl => lower.EndsWith(pl))) {
                            output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): Common name is a common plural");
                            knownPluralNames.Add(lower);

                            issueFound = true;
                        }
                    }
                }
            }


            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void PossiblePlurals() {
            output.WriteLine("==Possible plurals==");
            bool issueFound = false;

            string[] exceptions = "steenbras galaxias seps ss ops us mys is eros melidectes cinclodes 's andes texas charaxes".Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            exceptions = exceptions.Distinct().OrderBy(a => a).ToArray();
            // ss: bass, ass, cypress, albatross, grass, moss
            //string exceptions = "sweetlips", "galaxias", "seps"?

            output.WriteLine("Ignoring names ending with: " + exceptions.JoinStrings(", "));
            output.WriteLine();
            output.WriteLine("Most of these are false positives, but some might possibly be plurals which should be singular.");

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());
                foreach (string name in names) {
                    var lower = name.ToLowerInvariant();

                    if (!lower.EndsWith("s")) continue;
                    if (knownPluralNames.Contains(lower)) continue; // already done in known plurals list
                    if (exceptions.Any(lower.EndsWith)) continue;
                    if (lower.Contains("species code")) continue; // dealt with elsewhere

                    output.WriteLine("* ''" + bitri.FullName() + "'' (" + name + "): — possible plural");
                    issueFound = true;
                }

            }


            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void AllCaps() {
            output.WriteLine("==All caps==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (name.Any(char.IsLetter) && !name.Any(char.IsLower)) {
                        string correctCase = RedListCapsReport.CorrectCaps(name).UpperCaseFirstChar();
                        correctCase = correctCase.Replace("mediterranean", "Mediterranean"); // hack
                        output.WriteLine("* " + bitri.FullName() + " (" + name + ") — all caps name. Suggested: " + correctCase);
                        issueFound = true;
                    }
                }

            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void OddCaps() {
            output.WriteLine("==Odd caps==");
            bool issueFound = false;

            string[] exclusions = "De Mc Mac Van d' l'".Split().Distinct().OrderBy(a=>a).ToArray();

            output.WriteLine("Ignoring names starting with: " + exclusions.JoinStrings(", "));
            output.WriteLine();

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string namesField = bitri.CommonNameEng;
                if (namesField == null) continue;

                var names = namesField.Split(new char[] { ',' }).Select(m => m.Trim());

                foreach (string name in names) {
                    if (Regex.IsMatch(name, @"[a-z]\'[A-Z]")) { // lowercase ' Uppercase
                        if (exclusions.Any(ex => Regex.IsMatch(name, @"\b" + ex))) continue; // exlcusion must be found at start of a word (\b)
                        output.WriteLine("* " + bitri.FullName() + " (" + name + ") — odd caps with apostrophe");
                        issueFound = true;
                    } else if (Regex.IsMatch(name, @"[a-z][A-Z]")) {
                        if (exclusions.Any(ex => Regex.IsMatch(name, @"\b" + ex))) continue;
                        output.WriteLine("* " + bitri.FullName() + " (" + name + ") — camel case");
                        issueFound = true;
                    }
                }

            }
            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }


        public void SymbolsInScientificName() {
            output.WriteLine("==Symbols in scientific name==");
            bool issueFound = false;

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string name = bitri.FullName();
                if (name == null) continue;
                var okSymbols = @"' .""-".ToCharArray(); // ok symbols: ' space . " - 
                //var oddSymbols = @"!@#$%^&*_+[]/\|~:{};´".ToCharArray();
                //bool match = name.IndexOfAny(oddSymbols) != -1;
                bool match = name.Any(ch => (Char.IsSymbol(ch) || Char.IsPunctuation(ch)) && !okSymbols.Contains(ch)) ;
                if (!match) continue;
                // An ' may be found in, e.g. .. Chiloglanis sp. nov. 'Kerio'
                output.WriteLine("* ''" + name + "'' — contains symbol(s)");
                issueFound = true;
            }

            if (!issueFound) {
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }

        public void SpNov() {
            output.WriteLine("==Species Nova==");
            bool issueFound = false;

            int doubleQuote = 0;
            int singleQuote = 0;
            int empty = 0;
            int other = 0;

            string singleEg = "";
            string doubleEg = "";
            string emptyEg = "";
            string otherEg = "";

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                string name = bitri.FullName();
                if (name == null) continue;
                var match = Regex.Match(name, @"sp(ecies)?[\W]*nov(a)?[\W]", RegexOptions.IgnoreCase);
                if (!match.Success) continue;
                if (match.Value != "sp. nov.") {
                    output.Write("* ''" + name + "'' — contains odd variation of \"sp. nov.\"");
                    issueFound = true;
                }

                var doubleQuotes = Regex.Match(name, @""".*"""); // contains two double quotes
                var singleQuotes = Regex.Match(name, @"'.*'"); // contains two single quotes

                if (doubleQuotes.Success && singleQuotes.Success) {
                    output.WriteLine("* ''" + name + "'' — double and single quotes?");
                    issueFound = true;
                    other++;
                } else if (!doubleQuotes.Success && !singleQuotes.Success) {
                    if (name.EndsWith(match.Value)) {
                        // e.g. "Amomum sp. nov."
                        //output.WriteLine("* ''" + name + "'' — no sp. nov. name"); // not really an issue
                        emptyEg = name;
                        empty++;

                    } else {
                        // e.g. "Maytenus sp. nov. A"
                        //output.WriteLine("* ''" + name + "'' — no quotes"); 
                        //issueFound = true; // not really an issue
                        otherEg = name;
                        other++;
                    }

                } else if (doubleQuotes.Success) {
                    output.WriteLine("* ''" + name + "'' — uses double quotes (most use single quotes)"); // is it an issue?

                    doubleEg = name;
                    doubleQuote++;
                    issueFound = true;

                } else if (singleQuotes.Success) {
                    singleEg = name;
                    singleQuote++;
                }
                
            }

            output.WriteLine();
            output.WriteLine("Counts:");
            output.WriteLine("* Single quote: " + singleQuote + (singleEg == string.Empty ? "" : " e.g. " + singleEg));
            output.WriteLine("* Double quote: " + doubleQuote + (doubleEg == string.Empty ? "" : " e.g. " + doubleEg));
            output.WriteLine("* Empty: " + empty + (emptyEg == string.Empty ? "" : " e.g. " + emptyEg)); 
            output.WriteLine("* Other: " + other + (otherEg == string.Empty ? "" : " e.g. " + otherEg));

            if (!issueFound && (doubleQuote == 0 || singleQuote == 0)) {
                output.WriteLine();
                output.WriteLine("No issues found.");
            }
            output.WriteLine();
        }


    }
}