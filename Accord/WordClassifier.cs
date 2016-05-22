using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.Accord {
    class WordClassifier {

        /*
        Attempts to create a Hidden Markov Model for identifying binomial species names (including the space).

        Seems to recognise that "Ernest Hemingway" is not a species name, but is otherwise pretty shithouse.

        Not really sure if I'm doing it right.

Sample output:

>beastie ml
expected records: 2485491
record count (1): 2485491
record count (2): 2485491
Shuffling...
Creating hmm
Training hmm
Test results (highest probability to lowest):
input, result (pretty), result
*ae, -1.945543, -1.94554315684237
*aeaeaeaeaeae, -2.293293, -2.29329334276996
*aeaeae aeaeae, -2.341883, -2.3418828290351
*eeeeeeeeee, -2.465426, -2.46542555069967
*eeee eeee, -2.479427, -2.47942709837104
 Mesosa lata, -2.502344, -2.50234414158128
*Panaina panaina, -2.502928, -2.50292838211432
 Saintpaulia ionantha, -2.607934, -2.60793396375963
 Russula alpestris, -2.620368, -2.62036752270081
 Cincia conspersa, -2.625872, -2.62587183563775
*Stephania stephania, -2.665847, -2.66584740067794
*Aircraft Carrier, -2.688609, -2.68860889270155
 Poritia sebethis, -2.705786, -2.7057860219852
*Amputatio interilio-abdominalis, -2.740476, -2.74047571139696
 Carterocephalus albinotica, -2.753299, -2.75329917994517
*Good, -2.762275, -2.76227463265456
*Ph. cornutula, -2.769297, -2.76929679873798
 Alaptus bidentatus, -2.771766, -2.77176606950857
 Hemiphaisura gracilentor, -2.774781, -2.77478142083028
*Operculum corneous, -2.788008, -2.78800762967555
 Alcyonium adriaticum, -2.791319, -2.79131930539386
 Xenoceratias laevis, -2.803242, -2.80324186933143
*Dog cat, -2.803363, -2.80336327287119
 Goerodes omus, -2.809981, -2.809980539122
 Didymella ulicis, -2.825900, -2.82589955309607
*enough elements, -2.833172, -2.83317179786406
*Parsey McParseface, -2.845204, -2.84520356521248
 Schedonorus obtusus, -2.860564, -2.86056366350438
*prevailing usage, -2.880859, -2.88085894022728
 Limotettix glomerosa, -2.886977, -2.88697663168638
 Hypocalymma phillipsii, -2.893876, -2.89387610866681
*Tennis racquet, -2.903095, -2.90309522767588
 Ammoplanus chemehuevi, -2.913471, -2.91347116923806
 Dendrochilum microchilum, -2.935597, -2.93559695561556
*five items, -2.936010, -2.93601013909884
*asdfe asdgaretg, -2.957964, -2.95796411323517
 Polygala bryoides, -2.960595, -2.96059533287102
 Quedius gatenpio, -3.008249, -3.00824855190814
*mmmmmmmm, -3.013251, -3.01325068577805
*Whats up, -3.014462, -3.01446178853993
 Grewia fruticetorum, -3.037963, -3.03796345500927
*hello world, -3.041159, -3.04115887121507
*Ernest Hemingway, -3.043384, -3.04338354361053
*Hows things, -3.086421, -3.08642070726698
*Baum Welch, -3.124008, -3.12400794788661
*Whatever, -3.143979, -3.1439794806419
*qwertyuiop, -3.371561, -3.37156108917438
*qqqq qqqq, -5.575268, -5.5752678766736

    */
        public void BinomialTrainingTest() {
            //TODO: hmm.Save(filename), save the model
            //TODO: separate genus and species
            //TODO: use larger dataset (col + gni + wikis)
            
            // Time taken: 00:01:53.6373177, per sequence: 0.126263333s
            // Est.time for whole lot: 313826.37863s = 3.6 days

            string outputFile = string.Format(FileConfig.Instance().mostLatinLooking, "species"); ;

            SpeciesSet speciesSet = new SpeciesSet();
            speciesSet.ReadCol();
            var allSpecies = speciesSet.AllSpecies();
            Console.WriteLine("Shuffling...");
            allSpecies.Shuffle(); // Warning: this shuffles the underlying list in the SpeciesSet.

            var subset = allSpecies.Take(20000); // ignore everything else for now: 10000 takes 5 or 10 minutes. 1000 is relatively quick. 100 is fast for a test to see if it runs.
            
            int split = (int) (subset.Count() * 0.90);

            int[][] trainingSequences = subset.Take(split).Select(sp => SequenceUtil.StringToSeq(sp.ToString())).ToArray();
            var testingSequences = subset.Skip(split).Select(sp => sp.ToString()); // the remaining ones as strings
            string[] falseTests = { "Good", "Whatever", "hello world", "Aircraft Carrier", "Tennis racquet", "mmmmmmmm",
                "qqqq qqqq", "eeee eeee", "aeaeae aeaeae", "aeaeaeaeaeae", "eeeeeeeeee", "ae",
                "Whats up", "Hows things", "five items", "enough elements", "Baum Welch", "prevailing usage",
                "Ernest Hemingway", "Parsey McParseface", "Amputatio interilio-abdominalis", "Operculum corneous",
                "Panaina panaina", "Stephania stephania", "asdfe asdgaretg", "Ph. cornutula", "Dog cat", "qwertyuiop"};

            Console.WriteLine("Creating hmm");
            HiddenMarkovModel hmm = new HiddenMarkovModel(states: 12, symbols: 29);
            var trainingStopwatch = new Stopwatch();
            int count = trainingSequences.Length;
            trainingStopwatch.Start();
            Console.WriteLine("Training hmm with {0} sequences...", count);
            // Create a Baum-Welch learning algorithm to teach it
            BaumWelchLearning teacher = new BaumWelchLearning(hmm);

            // and call its Run method to start learning
            double error = teacher.Run(trainingSequences);

            trainingStopwatch.Stop();
            double timeEach = trainingStopwatch.ElapsedMilliseconds / 1000.0f / count;

            Console.WriteLine("Time taken: {0}, per sequence: {1}s", trainingStopwatch.Elapsed.ToString(), timeEach);
            Console.WriteLine("Est. time for whole lot: {0}s", timeEach * allSpecies.Count);

            Console.WriteLine("Test results (highest probability to lowest):");
            Dictionary<string, double> results = new Dictionary<string, double>();
            foreach (var test in testingSequences.Take(50)) {
                var seq = SequenceUtil.StringToSeq(test);
                double result = hmm.Evaluate(seq) / seq.Length;
                results[" " + test] = result;
                //CheckProbability(hmm, test.ToString());
            }

            foreach (var test in falseTests) {
                var seq = SequenceUtil.StringToSeq(test);
                double result = hmm.Evaluate(seq) / seq.Length;
                //TODO: also output Math.Exp() of raw result, which is meant to be the probabilty?
                results["*" + test] = result;
                //CheckProbability(hmm, test.ToString());
            }

            Console.WriteLine("input, result (pretty), result");
            foreach (var r in results.OrderByDescending(r => r.Value)) {
                
                //Console.WriteLine("{0}, {1:0.000000}, {2}", r.Key, r.Value, r.Value);
                Console.WriteLine("{0}, {1}", r.Key, r.Value);
            }

            Console.WriteLine("Writing results to: " + outputFile);
            var output = new System.IO.StreamWriter(outputFile);
            results = new Dictionary<string, double>();
            foreach (var sp in allSpecies) {
                string test = sp.ToString();
                var seq = SequenceUtil.StringToSeq(test);
                double result = hmm.Evaluate(seq) / seq.Length;
                results[test] = result;
                //CheckProbability(hmm, test.ToString());
            }

            output.WriteLine("input, result, training dataset: " + split);
            foreach (var r in results.OrderByDescending(r => r.Value)) {
                output.WriteLine("{0}, {1}", r.Key, r.Value);
            }
            output.Close();

        }


        void CheckProbability(HiddenMarkovModel hmm, string sequence) {
            double prob = Math.Exp(hmm.Evaluate(SequenceUtil.StringToSeq(sequence)));
            Console.WriteLine("input: '{0}', Probability: {1:0.0000} ({2})", sequence, prob, prob);
        }

        public void Test() {
            // Create a hidden Markov model with random parameter probabilities
            HiddenMarkovModel hmm = new HiddenMarkovModel(states: 5, symbols: 4);

            // Create an observation sequence of up to 2 symbols (0 or 1)
            int[] observationSequence = new[] { 0, 1, 1, 0, 0, 1, 1, 1 };

            // Evaluate its log-likelihood. Result is -5.5451774444795623
            double logLikelihood = hmm.Evaluate(observationSequence);

            // Convert to a likelihood: 0.0039062500000
            double likelihood = Math.Exp(logLikelihood);

            int[][] inputSequences =
    {
        new[] { 0, 1, 2, 3 },
        new[] { 0, 0, 0, 1, 1, 2, 2, 3, 3 },
        new[] { 0, 0, 1, 2, 2, 2, 3, 3 },
        new[] { 0, 1, 2, 3, 3, 3, 3 },
    };
            // Create a Baum-Welch learning algorithm to teach it
            BaumWelchLearning teacher = new BaumWelchLearning(hmm);

            // and call its Run method to start learning
            double error = teacher.Run(inputSequences);

            // Let's now check the probability of some sequences:
            double prob1 = Math.Exp(hmm.Evaluate(new[] { 0, 1, 2, 3 }));       // 0.013294354967987107
            double prob2 = Math.Exp(hmm.Evaluate(new[] { 0, 0, 1, 2, 2, 3 })); // 0.002261813011419950
            double prob3 = Math.Exp(hmm.Evaluate(new[] { 0, 0, 1, 2, 3, 3 })); // 0.002908045300397080

            // Now those obviously violate the form of the training set:
            double prob4 = Math.Exp(hmm.Evaluate(new[] { 3, 2, 1, 0 }));       // 0.000000000000000000
            double prob5 = Math.Exp(hmm.Evaluate(new[] { 0, 0, 1, 3, 1, 1 })); // 0.000000000113151816
        }
    }
}
