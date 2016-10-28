﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;
using MathNet.Numerics.LinearAlgebra;
using Accord.Math.Distances;
using Word2vec.Tools;
using MathNet.Numerics.LinearAlgebra.Single;

namespace beastie.WordVector {
    public class WordCluster {


        public void TestClusterizing() {
            VocabName vocabName = VocabName.GoogleNews_negative300;
            //VocabName vocabName = VocabName.glove_6B_300d; // smaller one?

            /*
            var db = BeastieDatabase.Database();
            var import = db.StartImport(jobAction, "SimilarWords", vocabName.ToString(), "WordDistancesData");
            if (import == null) {
                Console.WriteLine("SimilarWords job already complete on " + vocabName.ToString() + ". Use --force to restart it or something. (NYI). [or maybe there was an error]");
                return;
            }
            */

            Console.Error.WriteLine("Loading dictionary");

            bool normalize = true;
            NamedVocabulary namedVocab = NamedVocabulary.LoadNamed(vocabName, normalize);
            Vocabulary vocab = namedVocab.vocab; // OpenWordVecData();

            Clusterize(vocab);

        }

        /*
        example output:
        glove_6B_300d
        Cluster 0. Proportion: 8.0%. Nearest to centeroid: gmt, =, 0, km, p.m., seconds, cents, a.m., spokeswoman, yen, c., bc
        Cluster 1. Proportion: 37%. Nearest to centeroid: =, optional, 0, c., analyst, gmt, r, cents, yen, ap, &, d
        Cluster 2. Proportion: 15%. Nearest to centeroid: optional, =, r, yards, km, gmt, x, c., sen., bc, 0, yen
        Cluster 3. Proportion: 0.67%. Nearest to centeroid: novel, species, character, refer, ties, contributed, optional, album, taliban, d, mayor, diplomatic
        Cluster 4. Proportion: 4.0%. Nearest to centeroid: prosecutors, serb, bosnian, analyst, arafat, securities, spokeswoman, lawyers, crowd, denied, attorney, criticism
        Cluster 5. Proportion: 11%. Nearest to centeroid: refer, =, c., optional, &, r, editor, writer, professor, species, analyst, dr.
        Cluster 6. Proportion: 19%. Nearest to centeroid: km, =, c., &, 0, optional, index, railway, bc, gmt, scored, founded
        Cluster 7. Proportion: 4.7%. Nearest to centeroid: =, km, optional, cents, 0, gmt, index, analyst, c, yen, railway, /

        */

        public void Clusterize(Vocabulary vocab) {
            vocab.InitializeClusters();
            
        }
        public void ClusterizeWithAccord(Vocabulary vocab) {
            // How many clusters? https://en.wikipedia.org/wiki/Determining_the_number_of_clusters_in_a_data_set
            // 
            // A quick (and rough) method is to take the square root of the number of data points divided by two
            // -- https://www.quora.com/How-can-we-choose-a-good-K-for-K-means-clustering

            int take = 500;

            int count = vocab.Words.Count();
            //float[][] observations = vocab.Words.Select(w => w.NumericVector.ToArray()).ToArray();
            //double[][] observations = vocab.Words.Select(w => w.NumericVector.ToDouble().ToArray()).ToArray();
            double[][] observations = vocab.Words.Take(take).Select(w => w.NumericVector.ToDouble().ToArray()).ToArray();

            //var x = vocab.Words[0].NumericVector;

            //MathNet.Numerics.LinearAlgebra.Double.DenseVector d;

            //int k = (int) Math.Sqrt(count / 2);
            int k = (int)Math.Sqrt(take / 2);

            // cluster
            // https://github.com/accord-net/framework/blob/development/Sources/Accord.MachineLearning/Clustering/KMeans/KMeans.cs
            // vector
            // https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/LinearAlgebra/Single/DenseVector.cs
            // https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/LinearAlgebra/Double/DenseVector.cs
            // http://numerics.mathdotnet.com/api/MathNet.Numerics.LinearAlgebra.Double/Vector.htm

            Console.Error.WriteLine("Calculating kmeans. k=" + k);

            KMeans kmeans = new KMeans(k, new Cosine());
            kmeans.ComputeError = false; // faster?
            kmeans.ComputeCovariances = false; // faster?
            kmeans.Tolerance = 1e-3; // default is 1e-5
            kmeans.ParallelOptions.MaxDegreeOfParallelism = 6;
            kmeans.MaxIterations = 2; // glove_6B_300d, 2 iterations: 5 hrs

            var clusters = kmeans.Learn(observations);

            Console.Error.WriteLine("Done calculating kmeans");
            //clusters.Decide()
            /*
            var firstCluster = clusters[0];
            double[] centroid = firstCluster.Centroid;

            double[][] centroids = kmeans.Clusters.Centroids;
            double[] firstCentroid = centroids[0];
            var label = firstCluster.Index;
            */

            //foreach (var cluster in clusters) {
            for (int c=0; c<clusters.Count; c++) {
                var cluster = clusters[c];
                var centroid = cluster.Centroid;
                float[] centroidFloats = new float[centroid.Length];
                for (int i=0; i<centroid.Length; i++) {
                    centroidFloats[i] = (float)centroid[i];
                }
                //var centroidFloats = cluster.Centroid.AsEnumerable().Select(dd => (float)dd).ToArray(); // SZArrayEnumerator issues
                //var centroidRep = new Representation(centroidFloats);
                var centroidRep = new Representation(new DenseVector(centroidFloats).Normalize(2));
                Console.WriteLine("Cluster {0}. Proportion: {1}. Nearest to centeroid: {2}", cluster.Index, Blurb.Percent(cluster.Proportion), vocab.Distance(centroidRep, 12, 2000).Select(w => w.Representation.Word).JoinStrings(", "));
            }

            // find nearest
            //int c = kmeans.Clusters.Nearest(new double[] { 4, 1, 9) });


            // centroids to Vocabulary
            List<Representation> reps = new List<Representation>();
            for (int c = 0; c < clusters.Count; c++) {
                var cluster = clusters[c];
                var centroid = cluster.Centroid;
                float[] centroidFloats = new float[centroid.Length];
                for (int i = 0; i < centroid.Length; i++) {
                    centroidFloats[i] = (float)centroid[i];
                }
                //var centroidRep = new Representation(new DenseVector(centroidFloats).Normalize(2));
                var centroidRep = new CentroidRepresentation(c, new DenseVector(centroidFloats).Normalize(2));
                reps.Add(centroidRep);

            }

            // export words


        }
    }
}
