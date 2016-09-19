using beastie.DataModel;
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

namespace beastie.WordVector {
    public class DictionaryCreator {
        string zipDir = @"G:\archive\";

        public Vocabulary OpenWordVecData() {
            return NamedVocabulary.LoadNamed(VocabName.glove_twitter_27B_200d).vocab;
        }

        public void CreateSimiliarWordsListsFirstMillion() {
            int million = 1000000; // first million in vocab
            Vocabulary vocab = OpenWordVecData();
            foreach (var wordRep in vocab.Words.Take(500)) {
                string w = wordRep.Word;
                var similar = vocab.Distance(wordRep, 50);
                Console.WriteLine(w + ":");
                foreach (var neighbour in similar)
                    Console.WriteLine(" - " + neighbour.Representation.Word + " " + Blurb.Percent(neighbour.Distance));
            }
        }

        public void CreateSimiliarWordsLists(JobAction jobAction = JobAction.ContinueOrStartNewIfNone) {
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

                NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName);
                Vocabulary vocab = namedVocab.vocab; // OpenWordVecData();
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

                    if (namedVocab.isInvalidName(rawWord))
                        continue;

                    string cleanedWord = namedVocab.CleanWord(rawWord);

                    var similar = vocab.Distance(wordRep, 100);
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

                import.ImportSuccess();
            }

        }

        public void TestSimiliarWordsAndAnalogies() {
            
            VocabName vocabName = VocabName.GoogleNews_negative300;
            NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName);
            Vocabulary vocab = namedVocab.vocab; // OpenWordVecData();
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

                    var similar = vocab.Distance(wordRep, 100);

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

        public void CreateWordListsFromVocabs() {
            using (var db = BeastieDatabase.Database()) {

                //NOTE: sql to wipe data in table: "delete from wordsdata;" (or "TRUNCATE TABLE wordsdata;" in other sql)

                //TODO: vocab loading mode which doesn't normalize the data or do extra processing (or just for making word lists)

                var import = db.StartImport(JobAction.StartNewIfNone, "CreateWordListsFromVocab", "all wordvec vocabs", "WordsData");
                long importID = import.id;

                Console.WriteLine("Import id (CreateWordListsFromVocab): " + importID);

                foreach (VocabName vocabName in Enum.GetValues(typeof(VocabName))) {
                    if (vocabName == VocabName.None)
                        continue;

                    Console.WriteLine("Loading vocab: " + vocabName);
                    import.Log(db, "Loading vocab: " + vocabName);
                    NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName);

                    if (namedVocab == null) {
                        import.Log(db, "Skipping (failed to load): " + vocabName);
                        continue;
                    }
                    Vocabulary vocab = namedVocab.vocab; // OpenWordVecData();
                    import.Log(db, "Importing vocab: " + vocabName);
                    db.BeginTransaction();
                    foreach (var wordRep in vocab.Words) {
                        string rawWord = wordRep.Word;

                        //TODO: still add invalid to main word list?
                        if (namedVocab.isInvalidName(rawWord))
                            continue;

                        string cleanedWord = namedVocab.NormalizedWord(rawWord);  // namedVocab.CleanWord(rawWord);

                        //Console.WriteLine("adding: " + rawWord);
                        var wordsData = new WordsData();
                        wordsData.word = cleanedWord;
                        wordsData.wordRaw = rawWord; // TODO: only store if different to cleaned?
                        wordsData.source = vocabName.ToString();
                        wordsData.dataimport = importID;
                        db.Insert(wordsData);
                    }
                    db.CommitTransaction(); //TODO: catch and rollback

                }

                Console.WriteLine("Success");
                import.Log(db, "Success");

                //TODO: log number of words

                import.ImportSuccess();
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
