using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector
{
    public class TestPerformance
    {
        private int depth = 0;
        private FastVocabulary vocabulary;

        internal void Test() {
            ShowTime("Full performance test", () => RunTest() );


        }

        public void RunTest() {

            ShowTime("Load annoy vocab", () => LoadVocab() );
            vocabulary.limitTrees = 50;
            ShowTime("Nearest Neighbors (Annoy 50)", () => NearestNeighbours(vocabulary));
            vocabulary.limitTrees = 500;
            ShowTime("Nearest Neighbors (Annoy 500)", () => NearestNeighbours(vocabulary) );
            ShowTime("Nearest Neighbors (source)", () => NearestNeighbours(vocabulary.vocab));


        }

        void LoadVocab() {
            //Stopwatch loadVocab = ShowTimeStart();

            var annoyVocab = NamedVocabulary.LoadNamed(VocabName.glove_twitter_27B_200d_annoy500, false);
            vocabulary = (FastVocabulary)annoyVocab.vocab;

            //ShowTimeEnd(loadVocab, "Loading vocab");
            //return vocabulary;
        }

        void NearestNeighbours(IVocabulary vocabulary) {
            //string[] someWords = "kitten cause declaration brave always grow went gave agreeable directly".Split(' ');
            string[] someWords = "kitten cause always brave declaration".Split(' ');

            int count = 10;
            int i = 0;
            foreach (var word in someWords) {
                ShowTime(
                    string.Format("Word[{0}] \"{1}\"", i++, word),
                    () => NearestNeighbour(vocabulary, word, count));
            }
        }

        void NearestNeighbour(IVocabulary vocabulary, string word, int maxCount) {
            Console.Write(Indent());
            var rep = vocabulary[word];
            if (rep != null)
            {
                var closest = vocabulary.Nearest(rep, maxCount);
                //Console.WriteLine("top " + count + " closest to \"" + word + "\" words "); // (of first 100k words):");
                foreach (var neighbor in closest) {
                    // Console.WriteLine(neighbor.Representation.Word + "\t\t" + neighbor.Distance);
                    Console.Write(neighbor.Representation.Word + ", ");
                }
                Console.WriteLine();
            }
            else {
                Console.WriteLine("failed to find: " + word);
            }

        }

        void ApproximateNearestNeighbours() {

        }


        void ShowTime(string actionName, Action action) {
            var stopwatch = ShowTimeStart();
            action();
            ShowTimeEnd(stopwatch, actionName);
        }

        Stopwatch ShowTimeStart() {
            depth++;
            Stopwatch stopwatch = Stopwatch.StartNew();
            return stopwatch;
        }

        void ShowTimeEnd(Stopwatch stopwatch, string actionName)
        {
            stopwatch.Stop();
            depth--;
            Console.WriteLine("{0}{1}: {2}",
                Indent(),
                actionName,
                stopwatch.Elapsed);
        }

        private string Indent() {
            // new String('\t', depth),
            return string.Concat(Enumerable.Repeat("  ", depth));
        }


        static TimeSpan Time(Action action) {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
