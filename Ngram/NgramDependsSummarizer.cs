using beastie.beastieDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;

namespace beastie {
    public class NgramDependsSummarizer {
        public void CreateNgramDependTable() {

            //TODO: Convert entire thing into a database for easier access. Split off classifiers (_NOUN) into their own columns. Or maybe each word+classifier into its own table. Create views for basic analysis tasks.

            var beastieDb = BeastieDatabase.Database();
            //beastieDb.StartImport(JobAction) // TODO
            var import = beastieDb.StartImport(JobAction.StartNewIfNone, "NgramsDependsSummarizer", "googlebooks-eng-all-0gram-20120701", "NgramDepends");
            if (import == null) {
                Console.WriteLine("NgramsDependsSummarizer job already complete. Use --force to restart it [nyi]");
                return;
            }
            long importID = import.id;
            Console.WriteLine("Import id (NgramsDependsSummarizer): " + importID);

            
            //for now, limit to words in GoogleNews_negative300
            var lastsuc = beastieDb.GetLastSuccessfulImport("CreateWordList", "GoogleNews_negative300");
            var OkWordList = new HashSet<string>(beastieDb.WordsData.Where(w => w.dataimport == lastsuc.id).Select(w => w.wordRaw).Take(5000));
            OkWordList.Remove(string.Empty);
            var targetid = lastsuc.id;

            // a-z
            string filesTemplate = @"G:\ngrams\datasets-ngrams\googlebooks-eng-all-0gram-20120701-{0}.gz";
            var filenames = NgramFileIterator.AZ(filesTemplate);
            var streams = NgramFileIterator.OpenFiles(filenames);

            foreach (var stream in streams) {
                //beastieDb.BeginTransaction();
                Console.WriteLine("new file...");

                int minYear = 1950;
                //var groups = NgramFilterReader.ReadGroupedFiltered(stream, minYear);
                var groups = NgramFilterReader.ReadGrouped(stream); // no min year for now
                foreach (var group in groups) {
                    //Console.WriteLine("new group...");
                    var firstItem = group.FirstOrDefault();
                    if (firstItem == null) {
                        //Console.WriteLine("null");
                        continue;
                    }

                    var lemma = firstItem.lemma;
                    long total_match_count = group.Sum(ng => ng.match_count);
                    long total_volume_count = group.Sum(ng => ng.volume_count);

                    //not sure if forms without classifiers (eg _NOUN) also exist, so leave this off for now
                    //if (!lemma.isCanonical)
                    //    continue; // for now, ignore 

                    //string normalClean = BeastieDatabase.NormalizeForWordsData(lemma.cleaned); // numbers and punctuation removed, etc
                    string normalClean = BeastieDatabase.NormalizeForWordsData(lemma.noPos); // remove trailing _NOUN from raw lemma

                    string[] words = normalClean.Split(new string[] { @"=>" }, 2, StringSplitOptions.None);
                    if (words.Length != 2) {
                        Console.WriteLine("split fail: " + normalClean);
                        continue;
                    }
                    
                    string left = words[0];
                    string right = words[1];

                    //TODO: should only be checking current imports (or deleting old imports first)
                    //TODO: should be using WordsList table, but that hasn't been built yet.
                    //if (beastieDb.WordsData.Any(wd => wd.word == left) || beastieDb.WordsData.Any(wd => wd.word == right)) {
                    // wd.dataimport 27 = wordnet + sqlunetdb (gets no matches tho)
                    
                    //if (beastieDb.WordsData.Any(wd => wd.word == left && wd.dataimport == targetid) || beastieDb.WordsData.Any(wd => wd.word == right && wd.dataimport == targetid)) {
                    if (OkWordList.Contains(left) || OkWordList.Contains(right)) {
                        Console.WriteLine("* ADDING: {0} ({1}): {2} count", lemma.raw, lemma.cleaned, total_match_count);
                        // note, may create duplicates (with same left + right)
                        var deprow = new NgramDepend();
                        //TODO: include left and right of lemma.ScannoInsensitiveNormalized();
                        deprow.dataimport = importID;
                        deprow.left = left;
                        deprow.right = right;
                        deprow.lemma = lemma.raw;
                        deprow.match_count = total_match_count;
                        deprow.volume_count = total_volume_count;
                        // deprow.broader_match_count = total_match_count;
                        beastieDb.Update(deprow);
                    } else {
                        Console.WriteLine("no match: {0} ({1}): {2} count", lemma.raw, lemma.cleaned, total_match_count);
                    }

                    //TODO: 
                    //string scannoNormalized = lemma.ScannoInsensitiveNormalized();
                }

                //beastieDb.CommitTransaction(); // commit between streams

            }

        }
    }
}
