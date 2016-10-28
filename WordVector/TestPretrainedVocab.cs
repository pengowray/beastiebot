using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;
using System.Text.RegularExpressions;

namespace beastie.WordVector {
    class TestPretrainedVocab {

        public void DimensionExamples() {
            int topSmall = 10000; // 50000, 500000
            int topLarge = 200000; // 50000, 500000
            //foreach (VocabName name in Enum.GetValues(typeof(VocabName))) {
            foreach (string name in new string[] { VocabName.GoogleNews_negative300.ToString(), VocabName.glove_twitter_27B_25d.ToString() } ) {
                try {

                    //TODO: min/max/avg/std values (histogram) per dimension

                    Console.WriteLine("Vocab: " + name);
                    bool normalize = false;
                    var namedVocab = NamedVocabulary.LoadNamed(name, normalize);
                    if (namedVocab == null) continue;
                    var vocab = namedVocab.vocab;
                    int dimCount = vocab.VectorDimensionsCount;
                    for (int d = 0; d < dimCount; d++) {
                        float[] protypical = new float[dimCount];
                        protypical[d] = 1.0f;
                        var prototype = new WordRepresentation("d" + d, protypical);
                        var distancesNarrow = vocab.Distance(prototype, 50, topSmall);
                        var distancesWide = vocab.Distance(prototype, 50, topLarge);
                        var distancesFull = vocab.Distance(prototype, 50);
                        var max = vocab.Words.OrderByDescending(w => w.NumericVector[d]).Take(50);
                        var maxBroad = vocab.Words.Take(topLarge).OrderByDescending(w => w.NumericVector[d]).Take(50);
                        var maxNarrow = vocab.Words.Take(topSmall).OrderByDescending(w => w.NumericVector[d]).Take(50);
                        var min = vocab.Words.Take(topSmall).OrderBy(w => w.NumericVector[d]).Take(50);

                        Console.WriteLine(prototype.Word + ":");
                        Console.Write(" - prototype (narrow):");
                        foreach (var neighbour in distancesNarrow)
                            Console.Write(" - {0} {1}", neighbour.Representation.Word, Blurb.Percent(neighbour.Distance));
                        Console.WriteLine();

                        Console.Write(" - prototype (broad): ");
                        foreach (var neighbour in distancesWide)
                            Console.Write(" - {0} {1}", neighbour.Representation.Word, Blurb.Percent(neighbour.Distance));
                        Console.WriteLine();

                        Console.Write(" - prototype (full):  ");
                        foreach (var neighbour in distancesFull)
                            Console.Write(" - {0} {1}", neighbour.Representation.Word, Blurb.Percent(neighbour.Distance));
                        Console.WriteLine();

                        Console.Write(" - max (full):  ");
                        foreach (var neighbour in max)
                            Console.Write(" - {0} {1:n2}", neighbour.Word, neighbour.NumericVector[d]);
                        Console.WriteLine();

                        Console.Write(" - max (broad): ");
                        foreach (var neighbour in maxBroad)
                            Console.Write(" - {0} {1:n2}", neighbour.Word, neighbour.NumericVector[d]);
                        Console.WriteLine();

                        Console.Write(" - max (narrow):");
                        foreach (var neighbour in maxNarrow)
                            Console.Write(" - {0} {1:n2}", neighbour.Word, neighbour.NumericVector[d]);
                        Console.WriteLine();

                        Console.Write(" - min (narrow):");
                        foreach (var neighbour in min)
                            Console.Write(" - {0} {1:n2}", neighbour.Word, neighbour.NumericVector[d]);

                        Console.WriteLine();
                    }

                } catch (Exception e) {
                    Console.WriteLine("(error)");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void TestSkipGramPrefix() {
            var namedVocab = NamedVocabulary.LoadNamed(VocabName.freebase_skipgram1000_en, false);
            int count = 0;
            int mCount = 0;

            foreach(var word in namedVocab.vocab.Words) {
                if (word.Word.StartsWith("/en/")) {
                    count++;
                } else if (word.Word.StartsWith("/m/")) {
                    mCount++;
                } else { 
                    Console.WriteLine("odd: " + word.Word);
                }
            }
            int total = namedVocab.vocab.Words.Count();
            Console.WriteLine("{0} /en/ words of {1} found ({2}).", count, total, Blurb.Percent(count, total));
            Console.WriteLine("{0} /m/ words of {1} found ({2}).", mCount, total, Blurb.Percent(mCount, total));
        }

        int WordsCount(string neatened) {
            return neatened.Split(new char[] { '_', ' ' }, StringSplitOptions.RemoveEmptyEntries).Count();
        }

        public void TestAll() {
            foreach (VocabName name in Enum.GetValues(typeof(VocabName))) {
                //foreach (string name in new string[] { VocabName.glove_twitter_27B_25d.ToString(), VocabName.wikipedia_deps.ToString(), VocabName.en_1000_no_stem.ToString() }) {

                try {
                    Console.WriteLine("Vocab: " + name);
                    bool normalize = false;
                    //Console.WriteLine("Normalizing vectors: " + normalize);
                    var namedVocab = NamedVocabulary.LoadNamed(name, normalize);
                    if (namedVocab == null) continue;
                    var vocab = namedVocab.vocab;
                    Console.WriteLine(" - stated vs actual entries: {0}, {1} [{2}]", 
                        vocab.StatedVocabularySize, 
                        vocab.Words.Length,
                        vocab.StatedVocabularySize == vocab.Words.Length ? "OK" : "FAIL" );

                    Console.WriteLine(" - Vector Dimensions: {0}", vocab.VectorDimensionsCount);

                    Console.WriteLine(" - Random entry: {0}", vocab.Words.Random().Word);
                    //Console.WriteLine(" - Random entry 2: {0}", vocab.Words.Random().Word);
                    //Console.WriteLine(" - Random entry 3: {0}", vocab.Words.Random().Word);

                    Console.WriteLine(" - Unneatened Word Length. Min:{0}, Max:{1}, Avg:{2}",
                        vocab.Words.Min(w => w.Word.Length),
                        vocab.Words.Max(w => w.Word.Length),
                        vocab.Words.Average(w => w.Word.Length));

                    Console.WriteLine(" - Neatened Word Length. Min:{0}, Max:{1}, Avg:{2}",
                        vocab.Words.Min(w => namedVocab.Neaten(w.Word).Length),
                        vocab.Words.Max(w => namedVocab.Neaten(w.Word).Length),
                        vocab.Words.Average(w => namedVocab.Neaten(w.Word).Length));

                    var minWords = vocab.Words.Min(w => WordsCount(namedVocab.Neaten(w.Word)));
                    var maxWords = vocab.Words.Max(w => WordsCount(namedVocab.Neaten(w.Word)));
                    var avgWords = vocab.Words.Average(w => WordsCount(namedVocab.Neaten(w.Word))); // namedVocab.Neaten(w.Word).TrimEnd().Count(ch => ch == ' ')) + 1;
                    var moreThanOne = vocab.Words.Where(w => WordsCount(namedVocab.Neaten(w.Word)) > 1).Count();
                    Console.WriteLine(" - Number of words. Min:{0}, Max:{1}, Avg:{2}",
                        minWords,
                        maxWords,
                        avgWords);

                    Console.WriteLine(" - Entries with more than one word: {0} ({1})",
                        moreThanOne,
                        Blurb.Percent(moreThanOne, vocab.Words.Length));

                    Console.WriteLine(" - Max words example: {0}", vocab.Words.Where(w => WordsCount(namedVocab.Neaten(w.Word)) == maxWords).FirstOrDefault().Word);

                    //if (namedVocab.name == VocabName.glove_6B_300d.ToString() || namedVocab.name == VocabName.glove_6B_300d.ToString()) {
                    //    Console.WriteLine(" - All multi words entries: {0}", vocab.Words.Where(w => WordsCount(namedVocab.Neaten(w.Word)) > 1).Select(w => w.Word).JoinStrings(", "));
                    //}

                    Console.WriteLine(" - Metric Length. Min:{0}, Max:{1}, Avg:{2}",
                        vocab.Words.Min(w => w.MetricLength),
                        vocab.Words.Max(w => w.MetricLength),
                        vocab.Words.Average(w => w.MetricLength));

                    Console.WriteLine(" - Dimension values:  Min:{0}, Max:{1}, Avg:{2}",
                        vocab.Words.Min(w => w.NumericVector.Min()),
                        vocab.Words.Max(w => w.NumericVector.Max()),
                        vocab.Words.Average(w => w.NumericVector.Average()));

                    Console.WriteLine(" - Normalized values: Min:{0}, Max:{1}, Avg:{2}",
                        vocab.Words.Min(w => w.NumericVector.Normalize(2).Min()),
                        vocab.Words.Max(w => w.NumericVector.Normalize(2).Max()),
                        vocab.Words.Average(w => w.NumericVector.Normalize(2).Average()));

                    MinMaxAveragePercent(" - Vector zero values (exactly 0.0)",
                        vocab.Words.Min(w => w.NumericVector.Where(x => x == 0).Count()), 
                        vocab.Words.Max(w => w.NumericVector.Where(x => x == 0).Count()),
                        vocab.Words.Average(w => w.NumericVector.Where(x => x == 0).Count()),
                        vocab.VectorDimensionsCount);

                    float delta = 0.00001f;
                    MinMaxAveragePercent(" - Vector zero values (approx zero)",
                        vocab.Words.Min(w => w.NumericVector.Where(x => Math.Abs(x) < delta).Count()),
                        vocab.Words.Max(w => w.NumericVector.Where(x => Math.Abs(x) < delta).Count()),
                        vocab.Words.Average(w => w.NumericVector.Where(x => Math.Abs(x) < delta).Count()),
                        vocab.VectorDimensionsCount);

                    //TODO: may be broken
                    Console.WriteLine(" - Contains lowercase? {0}", vocab.Words.Any(w => namedVocab.Neaten(w.Word).Contains('a')));
                    Console.WriteLine(" - Contains uppercase? {0}", vocab.Words.Any(w => namedVocab.Neaten(w.Word).Contains('A')));
                    //Console.WriteLine(" - Contains any all caps (3 chars+)? {0}", vocab.Words.Select(w => namedVocab.Neaten(w.Word)).Any(s => regex) );

                    //TODO:
                    //Console.WriteLine(" - All characters contained: 

                    //TODO: search for prefixes ("/en/") and suffixes/tags ("|NOUN", "_NOUN_", "!NOUN" etc)
                    //List<string> suffixes = vocab.Words.Select(w => w.Word.Contains("|")).;

                    Console.WriteLine();



                } catch (Exception e) {
                    Console.WriteLine("(error)");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        static void MinMaxAveragePercent(string text, int min, int max, double average, int count) {
            Console.WriteLine("{0}: Min:{1} ({2}), Max:{3} ({4}), Avg:{5} ({6})",
                text,
                min, Blurb.Percent(min, count),
                max, Blurb.Percent(max, count),
                average, Blurb.Percent(average, count));

        }
    }
}
