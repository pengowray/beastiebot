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
        glove_twitter_27B_25d, // largely redundant
        glove_twitter_27B_200d, // 1,193,513 lowercase words and phrases
        glove_twitter_27B_200d_annoy500, // as above plus annoy index with 500 trees
        freebase_skipgram1000_en, // 1,422,903 lowercase words and phrases (1,261,101 /en/ lemmas)
        GoogleNews_negative300, // 2,999,997 mixed case words and phrases
        glove_6B_300d, // 400,000 lowercase words and phrases
        glove_42B_300d, // 1,917,494 lowercase words and phrases
        glove_840B_300d, // 2,196,007 lowercase words and phrases
        wikipedia_deps, // 174,015 lowercase words and phrases
        word_projections_640, // 82,390 uppercase words and "</s>"
        //rnn_rt07, // unused
        en_1000_no_stem, // unused. aka enwiki_gensim_word2vec_1000_nostem_10cbow
    }

    class NamedVocabulary { // TODO: better name for class?
        public string name;
        public IVocabulary vocab;

        public string prefix = string.Empty; // prefix needed to unneaten a lemma. But use Unneaten()
        public bool onlyLowercase = false; // are all entries only lowercase?
        public bool onlyUppercase = false; // are all entries only uppercase?
        public bool isNormalized = false; // are vectors normalized?

        public bool isIgnorableWord(string lemma) {
            if (string.IsNullOrWhiteSpace(lemma))
                return true;

            if (lemma.StartsWith("/m/")) // freebase ID (e.g. /m/0crf_n)
                return true;

            if (lemma.Length > 1 && !Regex.IsMatch(lemma, "[A-Za-z]"))  // require at least one plain alpha (if longer than 1 character)
                return true;

            //TODO: better filtering of email and web address
            string wordLower = lemma.ToLowerInvariant();
            string[] bad = { "www.", ".com", ".org", ".net", ".gov", ".uk", "http:", "https:", "://", "@" };
            if (bad.Any(b => wordLower.Contains(b)))
                return true;

            // how about...
            // <unk> <s> </s> ?

            return false;
        }

        // clean up the word for presentation. Reversable with Unneaten().
        public string Neaten(string unneatenedLemma) {
            string word = unneatenedLemma;

            if (word.StartsWith("DbpediaID/"))
                word = word.Substring("DbpediaID/".Length);

            if (word.StartsWith("/en/"))
                word = word.Substring("/en/".Length);

            word = word.Replace('_', ' ');

            if (onlyUppercase) {
                word = word.ToLowerInvariant();
            }

            //TODO: lowercase if all uppercase?

            return word;
        }

        public string ToNeatForSearch(string outsideLemma) {
            string word = outsideLemma;
            word = word.Replace('_', ' ');

            if (onlyLowercase)
                word = word.ToLowerInvariant();

            return word;
        }
        

        public string Unneaten(string neatenedLemma) {
            string word = neatenedLemma;

            if (onlyUppercase)
                word = word.ToUpperInvariant();

            word = prefix + word.Replace(' ', '_');

            return word;
        }

        /*
        public string NormalizedWord(string rawWord) {
            //Use BeastieDatabase.NormalizeForWordsData() instead
            return CleanWord(rawWord).Replace(" ", "").Replace("-","").ToLowerInvariant().Normalize();
        }
        */

        public NamedVocabulary() {
        }

        public NamedVocabulary(string name) {
            this.name = name;
        }

        public static NamedVocabulary LoadNamed(VocabName vocabId, bool normalize) {
            return LoadNamed(vocabId.ToString(), normalize);
        }

        public static NamedVocabulary LoadNamed(string vocabName, bool normalize) {
            //lists of pretrained models:
            // https://github.com/3Top/word2vec-api#where-to-get-a-pretrained-models
            // http://nlp.stanford.edu/projects/glove/

            string dir = @"G:\ngrams\datasets-wordvec\";
            string dir2 = @"C:\ngrams\datasets-wordvec\";

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

                // - Unneatened Word Length. Min:5, Max:165, Avg:18.6790062288153 (including /en/ prefix). 
                // - Neatened Word Length. Min:1, Max:161, Avg:15.1338566297211
                // - Number of words. Min:1, Max:21, Avg:2.26024121110153
                // - Entries with more than one word: 1089126 (77%)
                // - Max words example: /en/directive_on_the_coordination_of_certain_rules_concerning_copyright_and_rights_related_to_copyright_applicable_to_satellite_broadcasting_and_cable_retransmission

                // - Normalized vectors. Metric Length. Min:0.996692342974979, Max:0.997613084736035, Avg:0.99718374513929

                // - Dimension values:  Min:-0.1982422, Max:0.328125, Avg:9.375563E-05
                // - Normalized values: Min:-0.1982422, Max:0.328125, Avg:9.375563E-05 [already normalized]
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:1 (0.10%), Avg:1.40557718973113E-06 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:5 (0.50%), Avg:0.25033118912533 (0.03%)

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = true;
                vocab.vocab = Load(dir + @"freebase-vectors-skipgram1000-en.bin.gz", normalize, isNormalized);
                vocab.prefix = "/en/";
                vocab.onlyLowercase = true;
                //vocab.tags 
                return vocab;


            } else if (vocabName == VocabName.GoogleNews_negative300.ToString()) {
                // Google news
                // We are publishing pre-trained vectors trained on part of Google News dataset 
                // (about 100 billion words). 
                // The model contains 300-dimensional vectors for 3 million words and phrases. 
                // The phrases were obtained using a simple data-driven approach described in 
                // Tomas Mikolov, Ilya Sutskever, Kai Chen, Greg Corrado, and Jeffrey Dean. Distributed Representations of Words and Phrases and their Compositionality. In Proceedings of NIPS, 2013.

                // urls:

                // official:
                // https://drive.google.com/file/d/0B7XkCwpI5KDYNlNUTTlSS21pQmM/edit?usp=sharing
                // via: https://code.google.com/archive/p/word2vec/
                // mirror:
                // https://github.com/mmihaltz/word2vec-GoogleNews-vectors/raw/master/GoogleNews-vectors-negative300.bin.gz
                // via: https://github.com/mmihaltz/word2vec-GoogleNews-vectors

                //  - stated vs actual entries: 3000000, 2999997 [FAIL]
                //  - estimated 866 clusters [sqrt(k)/2]
                //  - numbers replaced with #
                //  - mixed case

                // example entries:
                // Ahmad_Aweidah
                // Xerox_Phaser_####DN
                // Patent_Nos._#,###,###
                // wasn'ta
                // Video_More_>>
                // eBay
                // Y2_###bn       // max one number per word?
                // UNIDENTIFIED WOMAN # // no solo numbers?
                // ####s
                // bars discos gymnasiums          // expressions which had commas have them removed

                // Number of words per entry. Min:0, Max:12, Avg:1.95900029233363
                // Entries with more than one word: 2070502 (69%)
                // 14 words: (perhaps literal underscores?)
                // #-###-###-####_begin_of_the_skype_highlighting_#-###-###-####_end_of_the_skype_highlighting
                // #-###-###-#### begin of the skype highlighting #-###-###-#### end of the skype highlighting
                // ...but typically 4 word max length...
                // YOUR LOCAL ANIMAL SHELTER, Lionhead_Studios_Peter_Molyneux
                // Billionaire investor Warren Buffett, Dalai Lama Tenzin Gyatso

                //- Word Length. Min:1, Max:98, Avg:13.7363224029891
                //- Not normalized: Metric Length. Min:0.0151729079029292, Max:21.1077051638769, Avg:2.04041050440455

                // - Dimension values:  Min:-4.0625, Max:4.1875, Avg:-0.003527858
                // - Normalized values: Min:-0.3398944, Max:0.342797, Avg:-0.001783744
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:9 (3.0%), Avg:0.0290693624026957 (0.01%)

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.vocab = Load(dir + @"GoogleNews-vectors-negative300.bin.gz", normalize, isNormalized);
                vocab.onlyLowercase = false;
                return vocab;

            } else if (vocabName == VocabName.glove_twitter_27B_25d.ToString()) {
                // Also available: 25d, 50d, 100d, 200d.
                // http://nlp.stanford.edu/data/glove.twitter.27B.zip / glove.twitter.27B.25d.txt
                // missing header: 1193513 25    -- <vocabularySize>space<vectorSize>
                //
                // Entries: 1,193,513
                // estimated 546 clusters?
                // - Word Length. Min:1, Max:140, Avg:6.7324964202317
                // - Metric Length. Min:0.0753421588130028, Max:17.8603267599632, Avg:4.77854079174945

                // - Dimension values:  Min:-10.575, Max:10.097, Avg:0.02476407
                // - Normalized values: Min:-0.9358753, Max:0.8314673, Avg:0.004433437
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:1 (4.0%), Avg:0.000218682159306183 (0.00%)

                // more details below (next entry, 200d)

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.vocab = LoadFromZip(dir + @"glove.twitter.27B.zip", "glove.twitter.27B.25d.txt", normalize, isNormalized);
                vocab.onlyLowercase = true;
                return vocab;

            } else if (vocabName == VocabName.glove_twitter_27B_200d.ToString() 
                || vocabName == VocabName.glove_twitter_27B_200d_annoy500.ToString() ) {
                // Also available: 25d, 50d, 100d.
                // http://nlp.stanford.edu/data/glove.twitter.27B.zip / glove.twitter.27B.200d.txt
                // Entries: 1,193,513
                // Word Length. Min:1, Max:140, Avg:6.7324964202317
                // missing header: 1193513 200

                // - Metric Length. Min:0.0279755592346191, Max:16.6470489501953, Avg:6.16954591421577

                // - Dimension values:  Min:-6.7986, Max:4.609, Avg:0.009065093
                // - Normalized values: Min:-0.9358753, Max:0.8314673, Avg:0.004433437
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:2 (1.0%), Avg:0.00375865197949247 (0.00%)

                // Number of words per entry. Min:0, Max:17, Avg:1.00669200921984
                // Entries with more than one word: 4874 (0.41%)

                // max words, 17:
                // سكس_طيز_قحبه_عنيف_اغتصاب_سكسيه_فحل_زب_نيك_بنات_مكوه_شهوه_لحس_عنف_تومبوي_ليدي_سبورت
                // سكس طيز قحبه عنيف اغتصاب سكسيه فحل زب نيك بنات مكوه شهوه لحس عنف تومبوي ليدي سبورت

                var nVocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;

                var annoyFile = dir2 + @"glove_twitter_27B\glove.twitter_200d_500trees.index";
                nVocab.vocab = LoadFromZip(dir + @"glove.twitter.27B.zip", "glove.twitter.27B.200d.txt", normalize, isNormalized);
                nVocab.onlyLowercase = true;

                if (vocabName == VocabName.glove_twitter_27B_200d_annoy500.ToString()) {
                    FastVocabulary annoyed = new FastVocabulary((Vocabulary)nVocab.vocab);
                    annoyed.LoadAnnoyIndex(annoyFile, IndexType.ANGULAR);
                    nVocab.vocab = annoyed;
                    return nVocab;
                }
                
                return nVocab;

            } else if (vocabName == VocabName.glove_6B_300d.ToString()) {
                // Wikipedia 2014 + Gigaword 5 (6B tokens, 400K vocab, uncased)
                // 300d. Also available: 50d, 100d, 200d, & 300d vectors

                // 400000 vocab.

                // - Word Length. Min:1, Max:68, Avg:7.3606775
                // - Not normalized. Metric Length. Min:0.0216130822974197, Max:17.1677576601073, Avg:6.5531258236344

                // - Dimension values:  Min:-3.0639, Max:3.2582, Avg:-0.003905012
                // - Normalized values: Min:-0.5268035, Max:0.376499, Avg:-0.0006353977
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:3 (1.0%), Avg:0.0063075 (0.00%)

                // glove.6B: Wikipedia 2014 + Gigaword 5 (6B tokens, 400K vocab, uncased, 50d, 100d, 200d, & 300d vectors, 822 MB download)
                // glove.6B: 1-grams. lowercase. no annotations.
                // glove.6B: contains email and web addressess e.g. http://www.kasparovchess.com, cyclingnews.com
                // numbers with other stuff: gev101, bb04, el1l, 4x4 [ok], hel101, 23aou94, 20,000-30, 4-million, 1905-1907, 12a
                // symbols: _____________, ------------------------------------------------------
                // numbers: 3.6730, 764-2815, 25-32, 6715/64001
                // repeated characters: ryryryryryry

                // mostly single words
                // - Number of words. Min:0, Max:3, Avg:1.0009925
                // - Entries with more than one word: 428 (0.11%)
                // - Max words example: dual_ec_drbg
                // - other multi examples: formula_1, goals_new, doubles_biggio, goal_colorado, batting_bonds, linesmen_greg, 2_6

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.vocab = LoadFromZip(dir + @"glove.6B.zip", "glove.6B.300d.txt", normalize, isNormalized);
                vocab.onlyLowercase = true;
                return vocab;

            } else if (vocabName == VocabName.glove_42B_300d.ToString()) {
                // Common Crawl (42B tokens, 1.9M vocab, uncased, 300d vectors)
                // - Actual entries: 1,917,494
                // - Word Length. Min:1, Max:1000, Avg:8.16323858118982
                // - Entries with more than one word: 9381 (0.49%)
                // - Not normalized. Metric Length. Min:0.0221787338858879, Max:16.3486152832491, Avg:4.96638407764034

                // - Dimension values:  Min:-5.0981, Max:3.2522, Avg:0.005720091
                // - Normalized values: Min:-0.7030315, Max:0.4699323, Avg:0.001358775
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:4 (1.3%), Avg:0.00909781204008983 (0.00%)

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.vocab = LoadFromZip(dir + @"glove.42B.300d.zip", "glove.42B.300d.txt", normalize, isNormalized);
                vocab.onlyLowercase = true;
                return vocab;

            } else if (vocabName == VocabName.glove_840B_300d.ToString()) {
                // Common Crawl (840B tokens, 2.2M vocab, cased, 300d vectors)
                // actual entries: 2,196,007
                // Word Length. Min:1, Max:1000, Avg:7.97268724553246
                // Entries with more than one word: 9232 (0.42%)
                // Not normalized. Metric Length. Min:0.0216177026065088, Max:26.180710961791, Avg:8.30302009427463

                // - Dimension values:  Min:-5.161, Max:5.0408, Avg:-0.005838563
                // - Normalized values: Min:-0.6151964, Max:0.6694535, Avg:-0.0007435827
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:0 (0.00%), Avg:0 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:6 (2.0%), Avg:0.00531828905827714 (0.00%)

                // includes words like: 
                // "storage.The", "#YouTube", "milesLEVEL", "mentor/mentee", "16-Jan-2009", "spanishcourses.info", 
                // "Europe/Africa", "AmazonAmazon", "slwoly", "great.We", "Colleges_and_Universities", "States12/01/2012", 
                // "19.jpg", "case.This", "increase.The", "times.Â", "CreamTreatmentsConditioner", "zitiertKlassifizierungen",
                // "EASTransportationEventsAll", "FUNdamental", "great-great-great-grandfather", "classificationsMeetings",
                // "structuresAlphabetical", "OpenUrlCrossRefMedlineWeb", "#firstworldproblems", "WarrantyFAQsCorporate"
                // "lab-tested"
                //
                //  - Number of words. Min:0, Max:15, Avg:1.00508559398945
                // 
                // Max words example: 
                // ctl00_XprLayout_cphRightSideContent_Right_xpr_layout_raisin_SearchInfoAccordion_01bb26a65_9859_5648_d6d1_46dde4cb35a_xprLNC_pCNp
                // ctl00 XprLayout cphRightSideContent Right xpr layout raisin SearchInfoAccordion 01bb26a65 9859 5648 d6d1 46dde4cb35a xprLNC pCNp

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.onlyLowercase = false;
                vocab.vocab = LoadFromZip(dir + @"glove.840B.300d.zip", "glove.840B.300d.txt", normalize, isNormalized);
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

                // - Number of words. Min:0, Max:8, Avg:1.00040226417263
                // - Entries with more than one word: 29 (0.02%)
                // - Max words example: historic_trails_and_roads_in_the_united_states

                // - Dimension values:  Min:-0.3454648, Max:0.2963374, Avg:0.0004948982
                // - Normalized values: Min:-0.3454648, Max:0.2963374, Avg:0.0004948982 [already normalized]
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:2 (0.67%), Avg:0.000724075510731833 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:2 (0.67%), Avg:0.0418986868948079 (0.01%)

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = true;
                //vocab.vocab = Load(dir + @"deps.words.bz2"); //TODO: .bz2 support -- original file: wikipedia_deps.bz2, 306 MB (320,870,380 bytes)
                vocab.vocab = Load(dir + @"deps.words.gz", normalize, isNormalized); //recompressed to .gz
                vocab.onlyLowercase = true;

                return vocab;
            
            } else if (vocabName == VocabName.word_projections_640.ToString()) {
                // http://rnnlm.org/ (recurrent neural network based language models)
                // Word projections from RNN-640 model trained on Broadcast news data 
                // also available: RNN-80 and extra large 1600-dimensional features from 3 models

                // - stated vs actual entries: 82390, 82390 [OK]
                // - Vector Dimensions: 640
                // - Random entry 1: RAGE
                // - Word Length. Min:1, Max:22, Avg:7.7739652870494
                // - Number of words. Min:1, Max:1, Avg:1
                // - Entries with more than one word: 0 (0.00%)
                // - Metric Length. Min:2.35890627619778, Max:83.4793258930767, Avg:8.83661379811971

                // - Dimension values:  Min:-20.96003, Max:34.28983, Avg:-0.009299609
                // - Normalized values: Min:-0.6087983, Max:0.8299806, Avg:0.0003245599
                // - Vector zero values (exactly 0.0): Min:0 (0.00%), Max:1 (0.16%), Avg:0.00133511348464619 (0.00%)
                // - Vector zero values (approx zero): Min:0 (0.00%), Max:3 (0.47%), Avg:0.0291661609418619 (0.00%)

                // example entries:
                // PAMUK'S, LAKSHAMANAN

                var vocab = new NamedVocabulary(vocabName);
                bool isNormalized = false;
                vocab.vocab = Load(dir + @"word_projections-640.txt.gz", normalize, isNormalized);
                vocab.onlyUppercase = true; // and "</s>"
                vocab.onlyLowercase = false;
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
                //vocab.isNormalized = ?;
                //vocab.prefix = "DbpediaID/";
                //vocab.onlyLowercase = false;
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
        public static Vocabulary LoadFromZip(string zipFile, string unzipFile = null, bool normalize = false, bool isNormalized = false) {
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
                            return new Word2VecTextReader(normalize, isNormalized).Read(path);
                        } else {
                            return new Word2VecBinaryReader(normalize, isNormalized).Read(path);
                        }
                    }
                }
            }
        }

        public static Vocabulary LoadGzip(string path, bool normalize, bool isNormalized) {
            if (!path.ToLowerInvariant().EndsWith(".gz"))
                return null; //TODO: throw error

            string nogzPath = path.Substring(0, path.Length - ".gz".Length);
            
            FileInfo fileToDecompress = new FileInfo(path);

            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                    string fileLower = nogzPath.ToLowerInvariant();
                    if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words")) { // .words is just for "deps.words" (wikipedia_deps)
                        return new Word2VecTextReader(normalize, isNormalized).Read(decompressionStream);

                    } else if (fileLower.EndsWith(".bin")) {
                        return new Word2VecBinaryReader(normalize, isNormalized).Read(decompressionStream);
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
        public static Vocabulary Load(string path, bool normalize, bool isNormalized) {
            string fileLower = path.ToLowerInvariant();
            if (fileLower.EndsWith(".txt") || fileLower.EndsWith(".words")) {
                return new Word2VecTextReader(normalize, isNormalized).Read(path);

            } else if (fileLower.EndsWith(".bin")) {
                return new Word2VecBinaryReader(normalize, isNormalized).Read(path);

            } else if (fileLower.EndsWith(".gz")) {
                return LoadGzip(path, normalize, isNormalized);

            } else if (fileLower.EndsWith(".zip")) {
                return LoadFromZip(path, null, normalize, isNormalized);
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
