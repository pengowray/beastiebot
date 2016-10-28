using Accord.MachineLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.WordVector {
    public class ClusterTester {
        public void Test() {



            // https://github.com/accord-net/framework/issues/325



            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };

            // Create a new K-Means algorithm with 3 clusters 
            KMeans kmeans = new KMeans(3);

            // Compute the algorithm, retrieving an integer array
            //  containing the labels for each of the observations
            //int[] labels = kmeans.Compute(observations);
            var clusters = kmeans.Learn(observations);

            // As result, the first two observations should belong to the
            // same cluster (thus having the same label). The same should
            // happen to the next four observations and to the last three.

            // In order to classify new, unobserved instances, you can
            // use the kmeans.Clusters.Nearest method, as shown below:
            int c = clusters.Decide(new double[] { 4, 1, 9 } );
            Console.WriteLine("answer: {0}", c);

            //OK
            for (int i = 0; i < clusters.Count; i++) {
                var cluster = clusters[i];
                Console.WriteLine("Cluster {0}: {1:0.00}%", cluster.Index, cluster.Proportion);
            }

            // Unhandled Exception: System.InvalidCastException: Unable to cast object of type 'SZArrayEnumerator' to type 'System.Collections.Generic.IEnumerator`1[Accord.MachineLearning.KMeansClusterCollection+KMeansCluster]'.
            foreach (var cluster in clusters) {
                Console.WriteLine("Cluster {0}: {1:0.00}%", cluster.Index, cluster.Proportion);
            }
        }

    }
}
