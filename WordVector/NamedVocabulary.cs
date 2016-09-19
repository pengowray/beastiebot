using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Word2vec.Tools;

namespace beastie.WordVector {

    // Files downloaded September 2016. Actual ages or date ranges covered by files largely unknown.

    public enum VocabName {
        None,
        glove_twitter_27B_25d,
        glove_twitter_27B_200d,
        freebase_skipgram1000_en,
        GoogleNews_negative300,
        glove_6B_300d,
        glove_42B_300d,
        glove_840B_300d,
        wikipedia_deps,
        word_projections_640,
        //rnn_rt07,
        en_1000_no_stem, // aka enwiki_gensim_word2vec_1000_nostem_10cbow
    }

    class NamedVocabulary { // TODO: better name for class?
        public string name;
        public Vocabulary vocab;

        public bool isInvalidName(string word) {
            if (string.IsNullOrWhiteSpace(word))
                return true;

            if (word.StartsWith("/m/")) // freebase ID (e.g. /m/0crf_n)
                return true;

            if (word.Length > 1 && !Regex.IsMatch(word, "[A-Za-z]"))  // require at least one plain alpha (if longer than 1 character)
                return true;

            //TODO: better filtering of email and web address
            string wordLower = word.ToLowerInvariant();
            string[] bad = { "www.", ".com", ".org", ".net", "http:", "https:", "://", "@" };
            if (bad.Any(b => wordLower.Contains(b)))
                return true;

            // how about...
            // <unk> <s> </s> ?

            return false;
        }

        public string CleanWord(string rawWord) {
            string word = rawWord;

            if (word.StartsWith("DbpediaID/"))
                word = word.Substring("DbpediaID/".Length);

            if (word.StartsWith("/en/"))
                word = word.Substring("/en/".Length);

            word = word.Replace('_', ' ');

            //TODO: lowercase if all uppercase

            return word;
        }

        public string NormalizedWord(string rawWord) {
            //TODO: unicode normalization
            return CleanWord(rawWord).Replace(" ", "").ToLowerInvariant();
        }

        public NamedVocabulary() {
        }

        public NamedVocabulary(string name) {
            this.name = name;
        }

        public static NamedVocabulary LoadNamed(VocabName vocabId) {
            return LoadNamed(vocabId.ToString());
        }

        public static NamedVocabulary LoadNamed(string vocabName) {
            //lists of pretrained models:
            // https://github.com/3Top/word2vec-api#where-to-get-a-pretrained-models
            // http://nlp.stanford.edu/projects/glove/

            string dir = @"G:\ngrams\datasets-wordvec\";

            if (vocabName == VocabName.freebase_skipgram1000_en.ToString()) {
                // More than 1.4M pre-trained entity vectors with naming from Freebase. 
                // This is especially helpful for projects related to knowledge mining.
                // date?

                // Entity vectors trained on 100B words from various news articles
                // Vectors sorted by frequency

                // Using the "deprecated /en/ naming"
                // e.g. /en/marvin_minsky
                // also using freebase IDs
                // e.g. /m/0crf_n

                // 1261101 /en/ words found (89%).
                //  161802 /m/ words found (11%).
                // 1422903 total
                // Freebase shut down: 16 December 2014

                //- Word Length (including /en/ prefix). Min:5, Max:165, Avg:18.6790062288153
                //- Normalized vectors. Metric Length. Min:0.996692342974979, Max:0.997613084736035, Avg:0.99718374513929

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = Load(dir + @"freebase-vectors-skipgram1000-en.bin.gz");
                //vocab.prefix = "/en/";
                //vocab.tags 
                return vocab;


            } else if (vocabName == VocabName.GoogleNews_negative300.ToString()) {
                // Google news
                // We are publishing pre-trained vectors trained on part of Google News dataset 
                // (about 100 billion words). 
                // The model contains 300-dimensional vectors for 3 million words and phrases. 
                // The phrases were obtained using a simple data-driven approach described in 
                // Tomas Mikolov, Ilya Sutskever, Kai Chen, Greg Corrado, and Jeffrey Dean. Distributed Representations of Words and Phrases and their Compositionality. In Proceedings of NIPS, 2013.

                // numbers replaced with #
                // mixed case

                //  - stated vs actual entries: 3000000, 2999997 [FAIL]

                // example entries:
                // Ahmad_Aweidah
                // Xerox_Phaser_####DN
                // Patent_Nos._#,###,###
                // wasn'ta
                // Video_More_>>
                // eBay

                //- Word Length. Min:1, Max:98, Avg:13.7363224029891
                //- Not normalized: Metric Length. Min:0.0151729079029292, Max:21.1077051638769, Avg:2.04041050440455

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = Load(dir + @"GoogleNews-vectors-negative300.bin.gz");
                return vocab;

            } else if (vocabName == VocabName.glove_twitter_27B_25d.ToString()) {
                // Also available: 25d, 50d, 100d, 200d.
                // Entries: 1,193,513
                // - Word Length. Min:1, Max:140, Avg:6.7324964202317
                // - Metric Length. Min:0.0753421588130028, Max:17.8603267599632, Avg:4.77854079174945

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = LoadFromZip(dir + @"glove.twitter.27B.zip", "glove.twitter.27B.25d.txt");
                return vocab;

            } else if (vocabName == VocabName.glove_twitter_27B_200d.ToString()) {
                // Also available: 25d, 50d, 100d.

                // Entries: 1,193,513
                // Word Length. Min:1, Max:140, Avg:6.7324964202317
                // Metric Length. Min:0.0279755790943608, Max:16.6470516492645, Avg:6.16954867773246

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = LoadFromZip(dir + @"glove.twitter.27B.zip", "glove.twitter.27B.200d.txt");
                return vocab;

            } else if (vocabName == VocabName.glove_6B_300d.ToString()) {
                // Wikipedia 2014 + Gigaword 5 (6B tokens, 400K vocab, uncased)
                // 300d. Also available: 50d, 100d, 200d, & 300d vectors

                // 400000 vocab.

                // - Word Length. Min:1, Max:68, Avg:7.3606775
                // - Not normalized. Metric Length. Min:0.0216130822974197, Max:17.1677576601073, Avg:6.5531258236344

                // glove.6B: Wikipedia 2014 + Gigaword 5 (6B tokens, 400K vocab, uncased, 50d, 100d, 200d, & 300d vectors, 822 MB download)
                // glove.6B: 1-grams. lowercase. no annotations.
                // glove.6B: contains email and web addressess e.g. http://www.kasparovchess.com, cyclingnews.com
                // numbers with other stuff: gev101, bb04, el1l, 4x4 [ok], hel101, 23aou94, 20,000-30, 4-million, 1905-1907, 12a
                // symbols: _____________, ------------------------------------------------------
                // numbers: 3.6730, 764-2815, 25-32, 6715/64001
                // repeated characters: ryryryryryry

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = LoadFromZip(dir + @"glove.6B.zip", "glove.6B.300d.txt");
                return vocab;

            } else if (vocabName == VocabName.glove_42B_300d.ToString()) {
                // Common Crawl (42B tokens, 1.9M vocab, uncased, 300d vectors)
                // - Actual entries: 1,917,494
                // - Word Length. Min:1, Max:1000, Avg:8.16323858118982
                // - Not normalized. Metric Length. Min:0.0221787338858879, Max:16.3486152832491, Avg:4.96638407764034
                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = LoadFromZip(dir + @"glove.42B.300d.zip", "glove.42B.300d.txt");
                return vocab;

            } else if (vocabName == VocabName.glove_840B_300d.ToString()) {
                // Common Crawl (840B tokens, 2.2M vocab, cased, 300d vectors)
                // actual entries: 2,196,007
                // Word Length. Min:1, Max:1000, Avg:7.97268724553246
                // Not normalized. Metric Length. Min:0.0216177026065088, Max:26.180710961791, Avg:8.30302009427463

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = LoadFromZip(dir + @"glove.840B.300d.zip", "glove.840B.300d.txt");
                return vocab;

            } else if (vocabName == VocabName.wikipedia_deps.ToString()) {
                // Wikipedia dependency, 300 dimensions
                // Vocabulary size: 174,015
                // Levy & Goldberg
                // architecture: word2vec 
                // training algorithm: modified word2vec
                // syntactic dependencies
                // Context window - size

                // - Random entry: tindal
                // - Word Length. Min:1, Max:47, Avg:7.48883142257851
                // - Metric Length. Min:1, Max:1, Avg:1 (normalized)

                var vocab = new NamedVocabulary(vocabName);
                //vocab.vocab = Load(dir + @"deps.words.bz2"); //TODO: .bz2 support -- original file: wikipedia_deps.bz2, 306 MB (320,870,380 bytes)
                vocab.vocab = Load(dir + @"deps.words.gz"); //recompressed to .gz

                return vocab;
            
            } else if (vocabName == VocabName.word_projections_640.ToString()) {
                // http://rnnlm.org/ (recurrent neural network based language models)
                // Word projections from RNN-640 model trained on Broadcast news data 
                // also available: RNN-80 and extra large 1600-dimensional features from 3 models

                // - stated vs actual entries: 82390, 82390 [OK]
                // - Vector Dimensions: 640
                // - Random entry 1: RAGE
                // - Word Length. Min:1, Max:22, Avg:7.7739652870494
                // - Metric Length. Min:2.35890627619778, Max:83.4793258930767, Avg:8.83661379811971

                var vocab = new NamedVocabulary(vocabName);
                vocab.vocab = Load(dir + @"word_projections-640.txt.gz");
                return vocab;

            } else if (vocabName == VocabName.en_1000_no_stem.ToString()) {
                // Enwiki Word2vec model 1000 Dimensions

                // tar.gz contains three files:
                // en.model -- binary with some text like "DBPEDIA_ID/Woodland_kingfisherr"
                // en.model.syn0.npy -- binary numpy file, with header: "93NUMPY☺ F {'descr': '<f4', 'fortran_order': False, 'shape': (1151090, 1000), }"
                // en.model.syn1.npy -- ditto

                // TODO: python script to convert to a normal vocab file
                // e.g. install anaconda (or python + numpy)
                // pip install -U gensim
                // then it fails
                // idk.. is python 3 even meant to exist? what happened to python being cross platform?
                // if python is so good at automating stuff why can't it automate the installation of a python script?
                // import numpy
                // numpy.load("en.model") 
                // numpy.load("en.model", "r", False) # but file does not contain non-pickled data

                //var vocab = new NamedVocabulary(vocabName);
                //vocab.vocab = Load(dir + @"en_1000_no_stem.tar.gz");
                //return vocab;

                // English Wikipedia (Feb 2015) 1000 dimension - No stemming - 10skipgram
                // https://github.com/idio/wiki2vec#prebuilt-models
                // https://github.com/idio/wiki2vec/raw/master/torrents/enwiki-gensim-word2vec-1000-nostem-10cbow.torrent

                // Articles are tokenized (At the moment in a very naive way)

                // if an article's text contains:
                // [[ Barack Obama | B.O ]] is the president of [[USA]]
                // is transformed into:
                // DbpediaID/Barack_Obama B.O is the president of DbpediaID/USA

                // This tokenization scheme is shit anyway. Should at least be "DbpediaID/USA USA" to keep it consistant (as [[USA|USA]] is semantically the same)
                return null;

            } else if (vocabName == "rnn_rt07") {
                // http://www.rnnlm.org/ -- not vectors (oops) -- 
                // "now integrated into Kaldi toolkit"

                //e.g.
                // 500	      4581	LEARN	500
                // 501	      4576	WONDER	501
                // 502	      4559	WALK	502
                // 503	      4539	TOWN	503

                //var vocab = new NamedVocabulary(vocabName);
                //vocab.vocab = LoadFromTarGz(dir + @"rnn-rt07-example.tar.gz", "rt09.rnn");
                //return vocab;

                return null;
            }

            return null;

        }

        //TODO: move to Vocabulary class in Word2vec.Tools
        public static Vocabulary LoadFromZip(string zipFile, string unzipFile = null) {
            using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Open)) {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read)) {
                    ZipArchiveEntry zipEntry;
                    if (string.IsNullOrEmpty(unzipFile)) {
                        // No filename provided, so use first file in zip.
                        zipEntry = archive.Entries[0];
                    } else {
                        zipEntry = archive.GetEntry(unzipFile);
                    }

                    using (Stream path = zipEntry.Open()) {
                        if (zipEntry.Name.ToLowerInvariant().EndsWith(".txt") || zipEntry.Name.ToLowerInvariant().EndsWith(".words")) {
                            return new Word2VecTextReader().Read(path);
                        } else {
                            return new Word2VecBinaryReader().Read(path);
                        }
                    }
                }
            }
        }

        public static Vocabulary LoadGzip(string path) {
            if (!path.ToLowerInvariant().EndsWith(".gz"))
                return null; //TODO: throw error

            string nogzPath = path.Substring(0, path.Length - ".gz".Length);
            
            FileInfo fileToDecompress = new FileInfo(path);

            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                    string fileLower = nogzPath.ToLowerInvariant();
                    if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words")) { // .words is just for "deps.words" (wikipedia_deps)
                        return new Word2VecTextReader().Read(decompressionStream);

                    } else if (fileLower.EndsWith(".bin")) {
                        return new Word2VecBinaryReader().Read(decompressionStream);
                    } 

                    return null; //TODO: examine file and guess whether binary or txt?
                }
            }

        }

        /// <summary>
        /// Opens .txt and .bin vocab files and compressed versions.
        /// Automatically selects best method to open files.
        /// Opens .txt.gz and .bin.gz, 
        /// If a zip contains only one .bin or .txt file, it will open that too, but better to use LoadGzip() instead
        /// </summary>
        /// <param name="path">Filename of file to load, including path</param>
        /// <returns></returns>
        public static Vocabulary Load(string path) {
            string fileLower = path.ToLowerInvariant();
            if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words")) {
                return new Word2VecTextReader().Read(path);

            } else if (fileLower.EndsWith(".bin")) {
                return new Word2VecBinaryReader().Read(path);

            } else if (fileLower.EndsWith(".gz")) {
                return LoadGzip(path);

            } else if (fileLower.EndsWith(".zip")) {
                return LoadFromZip(path);
            }
            
            //TODO: numpy (.npy)
            //TODO: .tar.gz
            //TODO: .bz2 

            //TODO: examine file and guess whether binary or txt?
            //TODO: throw error
            return null;
        }
    }
}
