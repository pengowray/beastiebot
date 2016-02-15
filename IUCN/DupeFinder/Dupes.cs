using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie {
    public class Dupes {
        public Dictionary<string, List<IUCNBitri>> allFoundNames = new Dictionary<string, List<IUCNBitri>>(); // <comparison string, list of matching>
        public Dictionary<string, string> dupes = new Dictionary<string, string>(); // output: <comparison string, example of non-mangled string>

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
            dupes.SortBestMatchFirst();

            return dupes;
        }

        public void SortBestMatchFirst() {
            // For all dupes, sort their bitris lists (in "allFoundNames") so the first item is an "exact" match or the best possible match.
            // an "exact" match is where the title matches the bitri, or otherwise the taxobox matches it
            //foreach (var dupe in dupes) {
            foreach (var dupe in dupes.OrderBy(e => e.Value)) {
                    List<IUCNBitri> list = null;
                if (allFoundNames.TryGetValue(dupe.Key, out list)) {
                    list.Sort((a, b) => StringComparer.InvariantCulture.Compare(a.BasicName(), b.BasicName())); // first sort by basic name

                    int matchIndex = -1;
                    for (int i = 0; i < list.Count(); i++) {

                        var bitri = list[i];
                        if (bitri.BasicName() == dupe.Key) {
                            // page name is the same as a bitri
                            matchIndex = i;
                            break;
                        }

                    }

                    if (matchIndex == -1) {
                        for (int i = 0; i < list.Count(); i++) {

                            var bitri = list[i];
                            var taxonName = bitri.TaxonName();
                            // check against taxobox taxon
                            string taxoboxName = taxonName.taxonField;
                            string basicName = bitri.BasicName();
                            //Console.WriteLine("Taxobox name: " + taxoboxName + ".. vs basic name:" + basicName );
                            if (taxoboxName != null && taxoboxName.Contains(basicName)) {
                                // matches scientific name used in taxobox 
                                //Console.WriteLine("Found within");
                                // TODO: note that some badly formatted taxoboxes contain have multiple scientific names.. watch out for these.
                                matchIndex = i;
                                break;
                            }
                        }
                    }

                    if (matchIndex == -1) {
                        for (int i = 0; i < list.Count(); i++) {
                            var bitri = list[i];
                            var taxonName = bitri.TaxonName();
                            // check specific epithet against taxobox taxon (last ditch effort)
                            string taxoboxName = taxonName.taxonField;
                            string epithet = bitri.epithet;
                            if (taxoboxName != null && taxoboxName.Contains(epithet)) {
                                // partially matches scientific name used in taxobox 
                                //Console.WriteLine("Found within I guess");
                                //TODO: consider trinomials better?
                                matchIndex = i;
                                break;
                            }
                        }
                    }

                    if (matchIndex != -1) {
                        var item = list[matchIndex];
                        list.RemoveAt(matchIndex);
                        list.Insert(0, item);
                        //Console.WriteLine("Moved item to front for: " + dupe.Key + " item: " + item.BasicName());
                    } else {
                        //Console.WriteLine("No best match: " + dupe.Key);
                    }
                }
            }
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

        public void ExportWithBitris(TextWriter output, string keyword = "dupe", Dupes alsoShow = null, bool wikiize = false) {

            if (alsoShow == null)
                alsoShow = alsoMatch; // default to showing these too.. TODO: really should show both

            Dictionary<string, List<IUCNBitri>> alsoShowThese = null;
            if (alsoShow != null) {
                alsoShowThese = alsoShow.allFoundNames;
            }

            foreach (var dupeEntry in dupes.OrderBy(d => d.Value)) { // .OrderBy(e => e.Value) // already sorted via SortBestMatchFirst()
                string dupeNomralized = dupeEntry.Key;
                string dupeExampleName = dupeEntry.Value;

                //Console.WriteLine(dupe);
                List<IUCNBitri> biList = null;
                List<IUCNBitri> triList = null;
                bool isBinom = allFoundNames.TryGetValue(dupeNomralized, out biList);
                bool isTrinom = false;
                if (alsoShowThese != null)
                    isTrinom = alsoShowThese.TryGetValue(dupeNomralized, out triList);

                string format = null;
                if (wikiize) {
                    format = "# [[{0}]] {1} ''{2}{3}{4}'' "; // legacy space on the end so it doesn't change from existing article
                } else {
                    format = "{0} {1} {2}{3}{4}";
                }

                string listString = string.Format(format,
                    dupeExampleName,
                    keyword, // dupeNomralized,
                    (isBinom ? biList.Select(bt => bt.FullName()).JoinStrings(", ") : ""),
                    (isBinom && isTrinom ? ", " : ""),
                    (isTrinom ? triList.Select(bt => bt.FullName()).JoinStrings(", ") : "")
                    );

                output.WriteLine(listString);
            }

        }

        public void SplitSpeciesSspLevel(out Dupes SpeciesDupes, out Dupes HigherDupes) {
            SplitLevels(new TaxonPage.Level[] { TaxonPage.Level.sp, TaxonPage.Level.ssp }, out SpeciesDupes, out HigherDupes);
        }
        public void SplitSspLevel(out Dupes SpeciesDupes, out Dupes HigherDupes) {
            SplitLevels(new TaxonPage.Level[] { TaxonPage.Level.ssp }, out SpeciesDupes, out HigherDupes);
        }

        public void SplitLevels(TaxonPage.Level[] spLevels, out Dupes SpeciesDupes, out Dupes HigherDupes) {
            SpeciesDupes = new Dupes();
            HigherDupes = new Dupes();

            SpeciesDupes.alsoMatch = alsoMatch;
            foreach (var item in allFoundNames) {
                var level = item.Value[0].TaxonName().pageLevel;
                bool isSpeciesLevel = spLevels.Contains(level); // (level == TaxonPage.Level.sp || level == TaxonPage.Level.ssp);
                Dupes bucket = (isSpeciesLevel ? SpeciesDupes : HigherDupes);
                bucket.allFoundNames[item.Key] = item.Value;
                if (dupes.ContainsKey(item.Key)) {
                    bucket.dupes[item.Key] = dupes[item.Key];
                }
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
//                    (isBinom ? biList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""),
//                    (isBinom && isTrinom ? ", " : ""),
//                    (isTrinom ? triList.Select(bt => bt.FullName()).OrderBy(a => a).JoinStrings(", ") : ""));
                    (isBinom ? biList.Select(bt => bt.FullName()).JoinStrings(", ") : ""), // ordered already by SortBestMatchFirst()
                    (isBinom && isTrinom ? ", " : ""),
                    (isTrinom ? triList.Select(bt => bt.FullName()).JoinStrings(", ") : ""));

                output.WriteLine(listString);
            }

        }


    }
}
