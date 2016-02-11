using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    class Dupes {
        public Dictionary<string, List<IUCNBitri>> allFoundNames; // <comparison string, list of matching>
        public Dictionary<string, string> dupes; // output: <comparison string, example of non-mangled string>

        public Dupes alsoMatch; // Dictionary<string, List<IUCNBitri>> alsoMatchThese = null

        // taxons which share a 
        public static Dupes FindByCommonNames( IEnumerable<IUCNBitri> bitris, Dupes alsoMatch = null) {
            // Dictionary<string, List<IUCNBitri>> alsoMatchThese = null) {
            //FindDupes(bitris, alsoMatchThese, AllCommonNamesNormalizer);

            Dupes dupes = new Dupes();
            dupes.alsoMatch = alsoMatch;
            dupes.FindDupes(bitris, AllCommonNamesNormalizerDictionary);

            return dupes;
        }

        // taxons which lead to the same page on Wikipedia
        public static Dupes FindWikiAmbiguous(IEnumerable<IUCNBitri> bitris, Dupes alsoMatch = null) {
            // Dictionary<string, List<IUCNBitri>> alsoMatchThese = null) {
            //FindDupes(bitris, alsoMatchThese, AllCommonNamesNormalizer);

            Dupes dupes = new Dupes();
            dupes.alsoMatch = alsoMatch;
            dupes.FindDupes(bitris, WikiPageNameNormalizer);

            return dupes;
        }


        //static IEnumerable<Tuple<string, string>> AllCommonNamesNormalizer(IUCNBitri bitri) { // Tuple: <normalized, example string>
        static Dictionary<string, string> AllCommonNamesNormalizerDictionary(IUCNBitri bitri) {

            Dictionary<string, string> newNames = new Dictionary<string, string>(); // <normalized name, an example of non-normalized name>
            string exampleName = bitri.TaxonName().CommonName(false);
            if (exampleName != null)
                newNames[exampleName.NormalizeForComparison()] = exampleName;

            // get other common names from iucn red list
            string[] iucnNames = bitri.CommonNamesEng();
            if (iucnNames != null) {
                foreach (string name in iucnNames) {
                    string norm = name.NormalizeForComparison();
                    if (norm == string.Empty)
                        continue;

                    newNames[norm] = name;
                }
            }

            return newNames;
        }

        //unused (doesn't give example string)
        static IEnumerable<string> AllCommonNamesNormalizer(IUCNBitri bitri) {

            HashSet<string> newNames = new HashSet<string>();
            string exampleName = bitri.TaxonName().CommonName(false);
            if (exampleName != null) {
                string normalized = exampleName.NormalizeForComparison();
                if (!string.IsNullOrEmpty(normalized)) {
                    newNames.Add(normalized);
                }
            }

            // get other common names from iucn red list
            string[] iucnNames = bitri.CommonNamesEng();
            if (iucnNames != null) {
                foreach (string name in iucnNames) {
                    string norm = name.NormalizeForComparison();
                    if (norm == string.Empty)
                        continue;

                    newNames.Add(norm);
                }
            }

            foreach (string name in newNames) {
                yield return name;
            }
        }

        //static IEnumerable<string> F(IUCNBitri bitri) {
        static Dictionary<string, string> WikiPageNameNormalizer(IUCNBitri bitri) {

            Dictionary<string, string> newNames = new Dictionary<string, string>(); // <normalized name, an example of non-normalized name>

            //string exampleName = bitri.TaxonName().CommonName(false);
            string pageTitle = bitri.TaxonName().pageTitle;

            if (!string.IsNullOrEmpty(pageTitle)) {
                //newNames[exampleName.NormalizeForComparison()] = exampleName;
                newNames[pageTitle.UpperCaseFirstChar()] = pageTitle;
            }

            return newNames;
        }

        public void FindDupes (
                IEnumerable<IUCNBitri> bitris,
                Func<IUCNBitri, Dictionary<string, string>> normalizer,
                bool showProgress = false
                /*
                out Dictionary<string, List<IUCNBitri>> allFoundNames, // output: <comparison string, list of matching>
                                                                       //out Dictionary<string, string> dupes, // output: <comparison string, example name>
                out HashSet<string> dupes,
                Dictionary<string, List<IUCNBitri>> alsoMatchThese,
                Func<IUCNBitri, IEnumerable<string>> normalizer,
                //Func<IUCNBitri, Dictionary<string, string>> normalizer, 
                */
                ) { // normalized string or wikipage name

            if (allFoundNames == null) {
                allFoundNames = new Dictionary<string, List<IUCNBitri>>();
            }

            if (dupes == null) {
                dupes = new Dictionary<string, string>();
            }

            //dupes = new HashSet<string>(); // normalized strings

            //Func<IUCNBitri, string> getComparisonString;
            Dictionary<string, List<IUCNBitri>> alsoMatchThese = null;
            if (alsoMatch != null) {
                alsoMatchThese = alsoMatch.allFoundNames;
            }

            foreach (IUCNBitri bitri in bitris) {

                // e.g. find if they conflict with other binoms or trinoms
                foreach (var item in normalizer(bitri)) {
                    //string normalized = item;
                    string normalized = item.Key;
                    string example = item.Value;

                    List<IUCNBitri> currentList = null;
                    if (allFoundNames.TryGetValue(normalized, out currentList)) {

                        // conflict found
                        dupes[normalized] = example;
                        //dupes.Add(normalized);
                        currentList.Add(bitri);
                        if (showProgress) {
                            Console.WriteLine("... Dupe found (normal): {0}. {2} & {3}",
                                normalized, bitri.FullName(), currentList[0].FullName());

                        }

                    } else {

                        // e.g. novel trinomial common name ...
                        currentList = new List<IUCNBitri>();
                        currentList.Add(bitri);
                        allFoundNames[normalized] = currentList;

                        // but it is it used for a binomial's common name?
                        if (alsoMatchThese != null && alsoMatchThese.ContainsKey(normalized)) {
                            // duplicate
                            dupes[normalized] = example;
                            if (showProgress) {
                                Console.WriteLine("... Dupe found (also match): {1}. {2} & {3}",
                                    normalized, example, bitri.FullName(), alsoMatchThese[normalized][0].FullName());
                            }
                        }
                    }
                }
            }

            //return dupes;

        }

        public void ExportWithBitris(TextWriter output, string keyword = "dupe", Dupes alsoShow = null) {

            if (alsoShow == null)
                alsoShow = alsoMatch; // default to showing these too.. TODO: really should show both

            Dictionary<string, List<IUCNBitri>> alsoShowThese = null;
            if (alsoShow != null) {
                alsoShowThese = alsoShow.allFoundNames;
            }

            foreach (var dupeEntry in dupes.OrderBy(e => e.Value)) {
                string dupeNomralized = dupeEntry.Key;
                string dupeExampleName = dupeEntry.Value;

                //Console.WriteLine(dupe);
                List<IUCNBitri> biList = null;
                List<IUCNBitri> triList = null;
                bool isBinom = allFoundNames.TryGetValue(dupeNomralized, out biList);
                bool isTrinom = false;
                if (alsoShowThese != null)
                    isTrinom = alsoShowThese.TryGetValue(dupeNomralized, out triList);
                
                string listString = string.Format("{0} {1} // {2}{3}{4} ",
                    dupeExampleName,
                    keyword, // dupeNomralized,
                    (isBinom ? biList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""),
                    (isBinom && isTrinom ? ", " : ""),
                    (isTrinom ? triList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""));

                output.WriteLine(listString);
            }

        }


        public void ExportWithBitrisSpeciesLevelPagesOnly(TextWriter output, string keyword = "is linked from", Dupes alsoShow = null, bool showSpeciesLevel = true) {
            if (alsoShow == null)
                alsoShow = alsoMatch; // default to showing these too.. TODO: really should show both

            Dictionary<string, List<IUCNBitri>> alsoShowThese = null;
            if (alsoShow != null) {
                alsoShowThese = alsoShow.allFoundNames;
            }

            foreach (var dupeEntry in dupes.OrderBy(e => e.Value)) {
                string dupeNomralized = dupeEntry.Key;
                string dupeExampleName = dupeEntry.Value;


                //Console.WriteLine(dupe);
                List<IUCNBitri> biList = null;
                List<IUCNBitri> triList = null;
                bool isBinom = allFoundNames.TryGetValue(dupeNomralized, out biList);
                bool isTrinom = false;
                if (alsoShowThese != null)
                    isTrinom = alsoShowThese.TryGetValue(dupeNomralized, out triList);

                if (isBinom) {
                    var level = biList[0].TaxonName().pageLevel;
                    if (showSpeciesLevel != (level == TaxonPage.Level.sp || level == TaxonPage.Level.ssp))
                        continue;
                } else {
                    continue; // not found wtf
                }

                string listString = string.Format("* [[{0}]] {1} ''{2}{3}{4}'' ",
                    dupeExampleName,
                    keyword, // dupeNomralized,
                    (isBinom ? biList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""),
                    (isBinom && isTrinom ? ", " : ""),
                    (isTrinom ? triList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""));

                output.WriteLine(listString);
            }

        }


    }
}
