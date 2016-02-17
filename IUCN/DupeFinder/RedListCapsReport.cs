using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class RedListCapsReport {
        Dictionary<string, string> wordToExample = new Dictionary<string, string>(); // lowercase word, example common name (first word in name not included)

        public void FindWords(TaxonNode topNode) {

            foreach (var bitri in topNode.DeepBitris().Where(bt => !bt.isStockpop)) {
                var taxonName = bitri.TaxonName();
                string name = taxonName.CommonName(); // must call this before checking commonNameFromIUCN
                if (!taxonName.commonNameFromIUCN)
                    continue;

                string lowercased = name.ToLowerInvariant();

                // split into words, with excessive checks for punctuation because it's from some weird stackoverflow example
                //var name = "'Oh, you can't help that,' said the Cat: 'we're all mad here. I'm mad. You're mad.'";
                var punctuation = lowercased.Where(Char.IsPunctuation).Distinct().ToArray();
                var words = lowercased.Split().Select(x => x.Trim(punctuation));

                if (words.Count() == 0)
                    continue;

                //NOTE/TODO: last word is almost never capitalized, except in rare exceptions:
                // * of: "Jewel of Burma", "Scaly snail of Doboz", "Queen of the Andes"
                // * de: "Jarendeua de Sapo" ?
                // * from: "River crayfish from the South" ?
                // * Douglas-fir: "Chinese Douglas-fir"
                bool hasOf = words.Contains("of") || words.Contains("de") || words.Contains("del") || words.Contains("di") || words.Contains("from");
                //bool hasDoug = words.Contains("douglas-fir");
                bool hasPunctuation = words.Last().Any(Char.IsPunctuation); // contains punctuation
                bool dontskiplast = hasOf || hasPunctuation;

                var relevantWords = words.Skip(1); // ignore first word (which is always title case for Wikipedia purposes)

                if (!dontskiplast) {
                    //Console.WriteLine("words count {0}, rel count {1}", words.Count(), relevantWords.Count());
                    relevantWords = relevantWords.Take(relevantWords.Count() - 1); // skip last word because it's probably going to be lowercase
                    //Console.WriteLine("...taken count {0}", relevantWords.Count());
                }

                foreach (var word in relevantWords) {
                    string correctedExample = CorrectCaps(name).UpperCaseFirstChar();

                    if (wordToExample.ContainsKey(word)) {
                        // append example
                        string existing = wordToExample[word];
                        if (existing.Length < 50) { // if there's already 50 characters of example, that's enough
                            wordToExample[word] = existing + ", " + correctedExample;
                        }

                    } else {
                        // add word with example
                        wordToExample[word] = correctedExample;
                    }
                }
            }

        }

        public void PrintWords(TextWriter output) {
            if (output == null)
                output = Console.Out;

            output.WriteLine("// Rename to (or replace) caps.txt to use capitalization from this file. Generated file preserves caps but not comments. ");

            var caps = TaxaRuleList.Instance().Caps;

            //TODO: append rules that are lost from (old) caps

            foreach (var entry in wordToExample.OrderBy(e => e.Key)) {
                string word = entry.Key;
                string example = entry.Value;

                string knownCaps = null;

                if (caps != null && caps.TryGetValue(word, out knownCaps)) {
                    output.WriteLine("{0} // {1}", knownCaps, example);
                } else {
                    output.WriteLine("{0} // {1}", word, example);
                }
            }

        }

        public static string CorrectCaps(string name) {
            var caps = TaxaRuleList.Instance().Caps;
            if (caps == null)
                return name;

            var punctuation = name.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = name.Split();

            StringBuilder result = new StringBuilder();
            foreach (string word in words) {
                string trimmed = word.Trim(punctuation);
                string lowered = trimmed.ToLowerInvariant();
                string knownCaps = null;
                bool useKnownCaps = (caps.TryGetValue(lowered, out knownCaps));
                string fixedCaps = (useKnownCaps ? knownCaps : lowered);

                if (trimmed == word) {
                    result.Append(fixedCaps);
                    result.Append(" ");

                } else {
                    //deal with punctuation
                    int start = word.IndexOf(trimmed);
                    string leftTrim = word.Substring(0, start);
                    string rightTrim = word.Substring(start + trimmed.Length);

                    result.Append(leftTrim);
                    result.Append(fixedCaps);
                    result.Append(rightTrim);
                    result.Append(" ");
                }
            }

            string final = result.ToString().TrimEnd();

            // if (final != name) Console.WriteLine("corrected caps: {0} => {1}", name, final);

            return final; // unoptimized trailing space removal
        }

        public static void ReadCapsToRules() {
            string filename = FileConfig.Instance().CapsReportFile + ".txt"; // note: remove '_generated' from filename for it to be read back
            Dictionary<string, string> caps = new Dictionary<string, string>(); // lowercase, corrected case. For IUCN Red List common names

            try {
                StreamReader capsReader = new StreamReader(filename, Encoding.UTF8);
                bool ok = true;
                while (ok) {
                    string line = capsReader.ReadLine();
                    if (line != null) {
                        // remove comment
                        if (line.Contains("//")) {
                            line = line.Substring(0, line.IndexOf("//"));
                        }
                        line = line.Trim();

                        // add to caps
                        string lower = line.ToLowerInvariant();
                        if (lower != line) {
                            caps[lower] = line;
                        }

                    } else {
                        ok = false;
                    }
                }

                capsReader.Close();

                TaxaRuleList.Instance().Caps = caps;

            } catch {
                Console.WriteLine("failed to read caps rule file: " + filename );
            }          
        }

    }

}