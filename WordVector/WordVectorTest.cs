using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector {
    public class WordVectorTest {

        //string zipDir = @"C:\ngrams\datasets-glove\";
        string zipDir = @"G:\ngrams\datasets-wordvec\";

        public Stream VectorTestData() {
            string zipPath = zipDir + "glove.840B.300d.zip";
            string unzipFile = "glove.840B.300d.txt";
            FileStream zipToOpen = new FileStream(zipPath, FileMode.Open);
            ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);
            ZipArchiveEntry zipEntry;
            if (string.IsNullOrEmpty(unzipFile)) {
                // No filename provided, so use first file in zip.
                zipEntry = archive.Entries[0];
            } else {
                zipEntry = archive.GetEntry(unzipFile);
            }
            Stream path = zipEntry.Open();

            return path;
        }

        public Stream VectorTestDataBinGz() {
            string path = zipDir + @"GoogleNews-vectors-negative300.bin.gz";
            FileInfo fileToDecompress = new FileInfo(path);
            FileStream originalFileStream = fileToDecompress.OpenRead();
            GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
            return decompressionStream;
        }

        public void Test(bool clusterize) {
            /* https://github.com/tmteam/Word2vec.Tools/blob/master/Word2vec.Tools.Example/Program.cs */
            string boy = "boy";
            string girl = "girl";
            string woman = "woman";

            var log = new LoggerTimer();
            log.Log("Start");

            //string boy = "happy";
            //string girl = "sad";
            //string woman = "life";

            /*
            string boy = "/en/boy";
            string girl = "/en/girl";
            string woman = "/en/woman";
            */

            //Set an w2v bin file path there:
            //string path = @"C:\Code\Corpus\DefaultGoogleVectors.bin";
            //var vocabulary = new Word2VecBinaryReader().Read(path);

            //string path = @"C:\ngrams\datasets-glove\glove.42B.300d\glove.42B.300d.txt";

            //Stream path = VectorTestData();
            //Stream path = VectorTestDataBinGz();

            //string path = zipDir + @"GoogleNews-vectors-negative300.bin";
            //string path = zipDir + @"freebase-vectors-skipgram1000-en.bin";

            Console.WriteLine("OK");

            log.Log("Reading vocab file");

            //var vocabulary = new Word2VecTextReader(false, false).Read(path);
            //var namedVocabulary = NamedVocabulary.LoadNamed(VocabName.GoogleNews_negative300, true);
            var namedVocabulary = NamedVocabulary.LoadNamed(VocabName.wikipedia_deps, true); // smallest

            var vocabulary = namedVocabulary.vocab;

            if (clusterize) {
                log.Log("Initializing clusters");
                vocabulary.InitializeClusters();
            }
            //var vocabulary = new Word2VecBinaryReader().Read(path);

            //For w2v text sampling file use:
            // var vocabulary = new Word2VecTextReader().Read(path);


            Console.WriteLine("vectors file: " + namedVocabulary.name);
            Console.WriteLine("vocabulary size: " + vocabulary.Words.Length);
            Console.WriteLine("w2v vector dimensions count: " + vocabulary.VectorDimensionsCount);

            Console.WriteLine();

            int count = 1000;

            log.Log("Distance");
            #region distance

            Console.WriteLine("top " + count + " closest to \"" + boy + "\" words:");
            var closest = vocabulary.Distance(boy, count);

            /* Is simmilar to:
            * var closest = vocabulary[boy].GetClosestFrom(vocabulary.Words.Where(w => w != vocabulary[boy]), count);
            */
            foreach (var neightboor in closest)
                Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance + "\t\t" + neightboor.Representation.cluster.Index);

            log.Log("Distance: teenager");
            var rep = vocabulary.GetRepresentationOrNullFor("teenager");
            if (rep != null) {
                closest = vocabulary.Distance(rep, count);
                Console.WriteLine("top " + count + " closest to \"" + "teenager" + "\" words "); // (of first 100k words):");
                foreach (var neightboor in closest)
                    Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance + "\t\t" + neightboor.Representation.cluster.Index);

            } else {
                Console.WriteLine("failed to find: " + "teenager");
            }


            #endregion

            Console.WriteLine();

            log.Log("Analogy");
            #region analogy
            Console.WriteLine("\"" + girl + "\" relates to \"" + boy + "\" as \"" + woman + "\" relates to ...");
            var analogies = vocabulary.Analogy(girl, boy, woman, count);
            foreach (var neightboor in analogies)
                Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);
            #endregion

            Console.WriteLine();

            log.Log("Addition");
            #region addition
            Console.WriteLine("\"" + girl + "\" + \"" + boy + "\" = ...");
            var additionRepresentation = vocabulary[girl].Add(vocabulary[boy]);
            var closestAdditions = vocabulary.Distance(additionRepresentation, count);
            foreach (var neightboor in closestAdditions)
                Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);
            #endregion

            Console.WriteLine();

            log.Log("Subtraction");
            #region subtraction
            Console.WriteLine("\"" + girl + "\" - \"" + boy + "\" = ...");
            var subtractionRepresentation = vocabulary[girl].Substract(vocabulary[boy]);
            var closestSubtractions = vocabulary.Distance(subtractionRepresentation, count);
            foreach (var neightboor in closestSubtractions)
                Console.WriteLine(neightboor.Representation.Word + "\t\t" + neightboor.Distance);
            #endregion

            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
            //GetCosineDistanceTo
            log.Log("Finished");
        }

    }
}
