using beastie.beastieDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Extensions;
using beastie.WordNet;

namespace beastie.WordVector {
    public class DictionaryCreator {
        string zipDir = @"G:\archive\";

        public IVocabulary OpenWordVecData() {
            return NamedVocabulary.LoadNamed(VocabName.glove_twitter_27B_200d, true).vocab;
        }

        public void CreateSimiliarWordsListsFirstMillion() {
            int million = 1000000; // first million in vocab
            IVocabulary vocab = OpenWordVecData();
            foreach (var wordRep in vocab.Words.Take(500)) {
                string w = wordRep.Word;
                var similar = vocab.Nearest(wordRep, 50);
                Console.WriteLine(w + ":");
                foreach (var neighbour in similar)
                    Console.WriteLine(" - " + neighbour.Representation.Word + " " + Blurb.Percent(neighbour.Distance));
            }
        }

        public void CreateSimiliarWordsLists(bool force = false) {
            JobAction jobAction = force ? JobAction.ForceStartNew : JobAction.ContinueOrStartNewIfNone;

            using (var db = BeastieDatabase.Database()) {
                VocabName vocabName = VocabName.GoogleNews_negative300;

                var import = db.StartImport(jobAction, "SimilarWords", vocabName.ToString(), "WordDistancesData");
                if (import == null) {
                    Console.WriteLine("SimilarWords job already complete on " + vocabName.ToString() + ". Use --force to restart it or something. (NYI). [or maybe there was an error]");
                    return;
                }
                long importID = import.id;
                bool continuing = (import.continuing == 1);

                Console.WriteLine("Import id: " + importID + (continuing ? " [resumed]" : " [new]"));
                Console.WriteLine("Loading vocab: " + vocabName);
                //TODO: handle System.Data.SQLite.SQLiteException: database is locked

                bool normalize = false; // TODO: maybe should have been normalized?
                NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName, normalize);
                var vocab = namedVocab.vocab; // OpenWordVecData();
                IEnumerable<WordRepresentation> iterate = vocab.Words;
                if (continuing && !string.IsNullOrEmpty(import.last_item_done)) {
                    string lastItem = import.last_item_done;
                    Console.WriteLine("Continuing from: " + lastItem);
                    iterate = iterate.SkipWhile(w => w.Word != lastItem);
                    //TODO: if not found, start from beginning

                } else {
                    Console.WriteLine("OK");
                }


                foreach (var wordRep in iterate) {
                    string rawWord = wordRep.Word;

                    if (namedVocab.isIgnorableWord(rawWord))
                        continue;

                    string cleanedWord = namedVocab.Neaten(rawWord);

                    var similar = vocab.Nearest(wordRep, 100);
                    SimilarWords sw = new SimilarWords(rawWord, similar);
                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(sw);

                    //TODO: if continuing, check if already has item and replace it
                    if (continuing) {
                        var existing = db.WordDistancesData.Where(r => r.dataimport == importID && r.word == cleanedWord).FirstOrDefault();
                        if (existing != null) {
                            Console.WriteLine("updating: " + rawWord);
                            existing.data = serialized;
                            //existing.word = cleanedWord;
                            db.Update(existing);
                            continue;
                        }
                    }

                    Console.WriteLine("adding: " + rawWord);
                    var distances_row = new WordDistancesData();
                    distances_row.word = cleanedWord;
                    distances_row.data = serialized;
                    distances_row.dataimport = importID;
                    db.Insert(distances_row);

                    import.last_item_done = rawWord; // TODO: maybe only update every 10 items or something
                    db.Update(import);

                    

                }

                //TODO: log number of words

                import.ImportSuccess(db);
            }

        }

        public void TestSimiliarWordsAndAnalogies() {
            
            VocabName vocabName = VocabName.GoogleNews_negative300;
            bool normalize = false;
            NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName, normalize);
            var vocab = namedVocab.vocab; // OpenWordVecData();
            var pairs = new Analogies().AnalogyPairsList();
                Random r = new Random();
                string[] interesting = Analogies.randomTestWords;

                foreach (var w in interesting) {
                    string word = w.ToLowerInvariant();
                    var wordRep = vocab.GetRepresentationOrNullFor(word); ;
                    if (wordRep == null) {
                        Console.WriteLine("Not found: " + w);
                        continue;
                    }

                    var similar = vocab.Nearest(wordRep, 100);

                    Console.WriteLine(w + ":");
                    foreach (var neighbour in similar)
                        Console.Write(" - {0} {1}", neighbour.Representation.Word, Blurb.Percent(neighbour.Distance));
                    Console.WriteLine();

                    int show = 6; // number of analogies to show
                    foreach (var p in pairs.Skip(r.Next(pairs.Count() - show)).Take(show)) {
                        var analogy = vocab.Analogy(p[0], p[1], word, 7);
                        Console.Write("{0}-{1}: ", p[0], p[1]);
                        if (analogy.Length <= 0) {
                            Console.Write("not found.");
                        } else {
                            foreach (var a in analogy) {
                                Console.Write("{0}-{1} {2}  ", word, a.Representation.Word, Blurb.Percent(a.Distance));
                            }
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }

        }

        public void CreateWordListsFromVocabs(bool force = false) {
            using (var db = BeastieDatabase.Database()) {

                //NOTE: sql to wipe data in table: "delete from wordsdata;" (or "TRUNCATE TABLE wordsdata;" in other sql)

                //TODO: vocab loading mode which doesn't normalize the data or do extra processing (or just for making word lists)
                var jobAction = force ? JobAction.ForceStartNew: JobAction.StartNewIfNone;

                var import = db.StartImport(jobAction, "CreateWordListsFromVocab", "all wordvec vocabs", "WordsData");
                if (import == null) {
                    Console.WriteLine("CreateWordListsFromVocab job already complete. Use --force to restart it. [or maybe there was an error]");
                    return;
                }
                long importID = import.id;
                Console.WriteLine("Import id (CreateWordListsFromVocab): " + importID);

                foreach (VocabName vocabName in Enum.GetValues(typeof(VocabName))) {

                    if (vocabName == VocabName.None)
                        continue;

                    Console.WriteLine("Loading vocab: " + vocabName);
                    import.Log(db, "Loading vocab: " + vocabName);

                    NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName, false);

                    if (namedVocab == null) {
                        import.Log(db, " - Skipping (failed to load): " + vocabName);
                        continue;
                    }

                    import.Log(db, "Loading vocab: " + vocabName);
                    var vocab = namedVocab.vocab; // OpenWordVecData();
                    var subImport = import.StartChildImport(JobAction.ForceStartNew, "CreateWordList", vocabName.ToString(), "WordsData"); //TODO: better resume support?
                    long subImportID = subImport.id;
                    import.Log(db, "Importing vocab: " + vocabName + ", id=" + subImportID);
                    Console.WriteLine(" - Sub-import id (CreateWordList): " + subImportID + " (" + vocabName + ")");

                    db.BeginTransaction();
                    foreach (var wordRep in vocab.Words) {
                        string rawWord = wordRep.Word;

                        //TODO: still add invalid to main word list?
                        if (namedVocab.isIgnorableWord(rawWord))
                            continue;
                        try {
                            string cleanedWord = BeastieDatabase.NormalizeForWordsData(namedVocab.Neaten(rawWord));  // namedVocab.CleanWord(rawWord);

                            //Console.WriteLine("adding: " + rawWord);
                            var wordsData = new WordsData();
                            wordsData.word = cleanedWord;
                            wordsData.wordRaw = rawWord; // TODO: only store if different to cleaned?
                            //wordsData.source = vocabName.ToString(); // get from dataimport
                            wordsData.dataimport = subImportID;
                            db.Insert(wordsData);
                        } catch (System.ArgumentException e) {
                            // System.ArgumentException: Invalid Unicode code point found at index 0
                            import.Log(db, "Bad word in "+ vocabName + ": \"" + rawWord +"\"");
                            subImport.Log(db, "Bad word: \"" + rawWord + "\"");
                            import.Log(db, e.Message);
                            subImport.Log(db, e.Message);
                        }
                    }
                    subImport.ImportSuccess(db);
                    db.CommitTransaction(); //TODO: catch and rollback

                }

                Console.WriteLine("Success");

                //TODO: log number of words

                import.ImportSuccess(db);
            }
        }

        public void ImportWordnetLemmas(bool force = false) {
            using (var db = BeastieDatabase.Database()) {
                var action = force ? JobAction.ForceStartNew : JobAction.StartNewIfNone;
                var import = db.StartImport(action, "ImportWordnetLemmas", "SqlunetDB.words", "WordsData");
                if (import == null) {
                    Console.WriteLine("ImportWordnetLemmas job already complete. Use --force to restart.");
                    return;
                }
                long importID = import.id;
                Console.WriteLine("Import id (ImportWordnetLemmas): " + importID);

                // which words table? 
                // casedwords = 40,850 (does not include all-lowercase words) e.g. "Edo", "Edvard Munch". has reference to words.wordid but are lowercase in words ("edvard munch")
                // vnwords = 4,153 words
                // xmwords = 116,246 definitions? (non-words)
                // words = 147,478 words

                db.BeginTransaction();
                using (var xdb = new SqlunetDB()) {
                    var xdb2 = new SqlunetDB(); // for subquery

                    foreach (var wordRecord in xdb.words) {
                        string lemma = (string) wordRecord.lemma;
                        if (string.IsNullOrEmpty(lemma)) continue;
                        string cased = (string) xdb2.casedwords.Where(w => w.wordid == wordRecord.wordid).FirstOrDefault()?.cased ?? lemma; //TODO: do a proper join

                        var wordsData = new WordsData();
                        wordsData.word = BeastieDatabase.NormalizeForWordsData(lemma);
                        wordsData.wordRaw = cased; // TODO: only store if different to cleaned?
                        //wordsData.source = "SqlunetDB.words";  // get from dataimport
                        wordsData.dataimport = importID;
                        db.Insert(wordsData);
                    }
                }
                import.ImportSuccess(db);
                db.CommitTransaction();
            }
        }


        public void ReadDistances() {
            using (var db = BeastieDatabase.Database()) {
                var q =
                    from c in db.WordDistancesData
                    select c;

                foreach (var c in q)
                    Console.WriteLine(c.data);
            }
        }

        public void CreateHTMLEntries() {

        }
    }
}
