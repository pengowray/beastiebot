using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector {
    public class TriangleInequalityTest {
        public void Test() {
            var log = new LoggerTimer();
            log.Log("Reading vocab file");
            var namedClusteredVocabulary = NamedVocabulary.LoadNamed(VocabName.wikipedia_deps, true);
            var vocab = namedClusteredVocabulary.vocab;

            //log.Log("Reading vocab file again");
            //var namedUnclusteredVocabulary = NamedVocabulary.LoadNamed(VocabName.wikipedia_deps, true);
            //var unclusteredVocab = namedUnclusteredVocabulary.vocab;

            //log.Log("Initializing clusters");
            //vocab.InitializeClusters();

            log.Log("Setting up");

            int count = 50;
            string word = "boy";

            Console.WriteLine("top " + count + " closest to \"" + word + "\" words:");
            var rep = vocab.GetRepresentationOrNullFor(word);

            log.Log("Simple angle distance");
            var simpleAngleDists = vocab.EuclideanNearest(rep, count);
            PrintDistances("Simple angle distance", simpleAngleDists);

            log.Log("Distance (no clusters)");
            var noClustersDists = rep.NearestFrom(vocab.Words, count);
            PrintDistances("Distances (no clusters)", noClustersDists);

            log.Log("Distance (clusters)");
            var clusteredDists = vocab.Nearest(rep, count);
            PrintDistances("Distances (clusters)", clusteredDists);

            /*
            log.Log("Find missing words");
            Console.WriteLine("Base word: {0}", rep.Word);
            Console.WriteLine(" - Cluster: {0}", rep.cluster.Index);
            Console.WriteLine(" - Cluster radius (sangle): {0}", rep.cluster.Radius);
            Console.WriteLine(" - Cosine distance from centroidword: {0}", rep.cluster.Centroid.GetCosineDistanceTo(rep));
            //Console.WriteLine(" - Cosine distance from word to centroid: {0}", rep.GetCosineDistanceTo(rep.cluster.Centroid));
            Console.WriteLine(" - Sangle distance from centroid: {0}", rep.cluster.Centroid.GetSimpleAngleTo(rep));

            Console.WriteLine("Nearest clusters: centroid-dist, word-cluster-min-dist");
            foreach (var cl in vocab.MinClusterDistaces(rep)) { // rep.cluster.Nearest) {
                var cdist = rep.GetSimpleAngleTo(cl.Cluster.Centroid);
                var betterMinDist = cdist - cl.Cluster.Radius; // == cl.MinDistance
                Console.WriteLine(" - cluster {0}: {1}, {2}", cl.Cluster.Index, cdist, betterMinDist);
                //Console.WriteLine(" - Distance (cosine): {0}", .GetCosineDistanceTo(noClustersDists.Last().Representation));
                //Console.WriteLine(" - min cluster-cluster (sangle): {0}", rep.cluster.Centroid.GetSimpleAngleTo(cWord.Representation.cluster.Centroid) - rep.cluster.Radius - cWord.Representation.cluster.Radius);
            }
            */

            Console.WriteLine("Final word: {0}", noClustersDists.Last().Representation.Word);
            Console.WriteLine(" - Distance (cosine): {0}", rep.GetCosineDistanceTo(noClustersDists.Last().Representation));
            Console.WriteLine(" - Distance (sangle): {0}", rep.GetSimpleAngleTo(noClustersDists.Last().Representation));

            HashSet<string> clusteredWords = new HashSet<string>(clusteredDists.Select(cl => cl.Representation.Word));;
            foreach (var cWord in noClustersDists) {
                if (!clusteredWords.Contains(cWord.Representation.Word)) {
                    Console.WriteLine("Missing word: {0}", cWord.Representation.Word);
                    //Console.WriteLine(" - Cluster: {0}", cWord.Representation.cluster.Index);
                    Console.WriteLine(" - Distance (cosine): {0}", rep.GetCosineDistanceTo(cWord.Representation));
                    Console.WriteLine(" - Distance (sangle): {0}", rep.GetSimpleAngleTo(cWord.Representation)); // simple angle / euclidean
                    //Console.WriteLine(" - cluster-cluster (cosine): {0}", rep.cluster.Centroid.GetCosineDistanceTo(cWord.Representation.cluster.Centroid));
                    //Console.WriteLine(" - cluster-cluster (sangle): {0}", rep.cluster.Centroid.GetSimpleAngleTo(cWord.Representation.cluster.Centroid));
                    //Console.WriteLine(" - min cluster-cluster (cosine/sangle): {0}", rep.cluster.Centroid.GetCosineDistanceTo(cWord.Representation.cluster.Centroid) - rep.cluster.Radius - cWord.Representation.cluster.Radius);
                    //Console.WriteLine(" - min cluster-cluster (sangle): {0}", rep.cluster.Centroid.GetSimpleAngleTo(cWord.Representation.cluster.Centroid) - rep.cluster.Radius - cWord.Representation.cluster.Radius);
                    //Console.WriteLine(" - min word-cluster (sangle): {0}", rep.GetSimpleAngleTo(cWord.Representation.cluster.Centroid) - rep.cluster.Radius - cWord.Representation.cluster.Radius);
                }
            }
            
            log.Log("Done");
        }

        public void PrintDistances(string title, WordDistance[] dists) {
            if (title != null) {
                Console.WriteLine(title + ":");
            }

            foreach (var word in dists) {
                Console.WriteLine(word.Representation.Word + "\t" + word.Distance); //  + "\t" + word.Representation.cluster.Index);
            }
        }
    }
}
