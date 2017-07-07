using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using System.Text;
using beastie.Accord;
using beastie.WordVector;
using beastie.Wiktionary;

namespace beastie
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			string verb = null;
			object verbInstance = null;
			var options = new Options();

			if (!CommandLine.Parser.Default.ParseArguments(args, options, 
				(verbArg, subOptionsArg) =>
				{
					// if parsing succeeds the verb name and correct instance
					// will be passed to onVerbCommand delegate (string,object)
					verb = verbArg;
					verbInstance = subOptionsArg;
				}))
			{
				Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
			}

            if (verb == "mysqld") {
                //var suboptions = (MysqldSubOptions)verbInstance; // same same
                var suboptions = options.MysqldVerb;

                RunMysqld me = new RunMysqld();
                string year = suboptions.year;
                if (year != null && year != "") {
                    me.year = year; // "2014"
                }

                Console.WriteLine("location: " + me.ColLocation());
                if (suboptions.shutdown) {
                    me.ShutdownDatabase();
                } else {
                    if (suboptions.DontRunNewMysqld) {
                        Console.WriteLine("You want me to start it or not?");
                    } else {
                        me.StartDatabase();
                    }
                }
            } else if (verb == "wiktionary-db-extras-setup") {
                // for now assumes you've got docker image running already with wiktionary data imported already 
                // see docker google doc

                // see also:
                //ImportWiktionaryDatabase();

                var suboptions = options.WiktionaryDatabaseExtras;

                var wiktUtil = new WiktionaryDatabaseUtilities();
                wiktUtil.BuildEverything();

                //tests:
                WordLangs("croissant");
                WordLangs("cat");
                WordLangs("dog");
                Console.ReadKey();

            } else if (verb == "build-species-table") {
                //var suboptions = (CommonSubOptions)verbInstance; // works too
                var suboptions = options.BuildSpeciesTable;

                var coldb = CatalogueOfLifeDatabase.Instance();
                coldb.dontStartMysqld = suboptions.DontRunNewMysqld;
                coldb.BuildSpeciesTable();

            } else if (verb == "filter-2gram-species") {
                var suboptions = options.FilterNgramSpecies;
                string outputFile = suboptions.outputFile;
                bool append = suboptions.append;

                var filter = new NgramSpeciesFilter();
                filter.LoadSpeciesSet();

                //TODO: move this to another verb
                Console.WriteLine("All Species list first characters: ");
                Console.WriteLine(filter.species.AllFirstChars());
                Console.WriteLine("Other characters: ");
                Console.WriteLine(filter.species.AllOtherChars());
                filter.SetOutputFile(outputFile, append);

                //filter.ReadFile(@"D:\ngrams\datasets\two-word examples.txt"); // test
                //filter.ReadFile(@"D:\ngrams\datasets\2gram\googlebooks-eng-all-2gram-20120701-bo.gz");
                //filter.ReadFile(@"D:\ngrams\datasets\2gram\googlebooks-eng-all-2gram-20120701-ra.gz");
                //filter.ReadFile(@"D:\ngrams\datasets\2gram\googlebooks-eng-all-2gram-20120701-bo");

                string template = @"http://storage.googleapis.com/books/ngrams/books/googlebooks-eng-all-2gram-20120701-{0}.gz";
                filter.ReadUrisAAZZ(template);

                filter.Close();

                filter.CopyFileToS3(outputFile);

                // replace "??" with aa, ab, etc (skips words starting with punctuation, etc)

                //filter.ReadUri(@"http://storage.googleapis.com/books/ngrams/books/googlebooks-eng-all-2gram-20120701-aa.gz");
            } else if (verb == "wiki-missing-species-all") {
                // WikiMissingSpeciesAll
                new WikiMissingSpeciesAll().Run();

            } else if (verb == "wiki-missing-species") {
                // was: "tally-species -w" (suboptions.wikiStyleOutput)

                var suboptions = options.WikiMissingSpecies;
                string outputFile = suboptions.outputFile;

                var tally = new WikiMissingSpecies();
                tally.onlyNeedingWikiArticle = suboptions.onlyNeedingWikiArticle;

                if (suboptions.since != null) { //  && suboptions.since != 0
                    tally.startYear = (int)suboptions.since;
                } else {
                    tally.startYear = 1950;
                }
                Console.WriteLine("tally.startYear: " + tally.startYear);

                if (string.IsNullOrEmpty(outputFile)) {
                    // default file name
                    string kingdom = string.IsNullOrEmpty(suboptions.kingdom) ? "" : "-" + suboptions.kingdom;
                    string class_ = string.IsNullOrEmpty(suboptions.class_) ? "" : "-" + suboptions.class_;
                    string todo = tally.onlyNeedingWikiArticle ? "-todo" : "";
                    string sinceyear = "-post" + tally.startYear;

                    outputFile = FileConfig.datadir + string.Format(@"output-wiki\species-wiki{0}{1}{2}{3}.txt", kingdom, class_, sinceyear, todo);

                }

                if (!string.IsNullOrEmpty(suboptions.kingdom)) {
                    tally.kingdom = suboptions.kingdom;
                }

                if (!string.IsNullOrEmpty(suboptions.class_)) {
                    tally.class_ = suboptions.class_;
                }

                tally.ReadFile();
                tally.Close();
                tally.WikiListToFile(outputFile);

            } else if (verb == "tally-species") {
                var suboptions = options.TallySpecies;
                string outputFile = suboptions.outputFile;

                var tally = new NgramSpeciesTally();

                if (suboptions.since != null) { //  && suboptions.since != 0
                    tally.startYear = (int)suboptions.since;
                } else {
                    tally.startYear = 1950;
                }
                Console.WriteLine("tally.startYear: " + tally.startYear);

                if (string.IsNullOrEmpty(outputFile)) {
                    string sinceyear = "-post" + tally.startYear;

                    //FileConfig.datadir + string.Format(@"datasets-generated\col-species-in-eng-all-2gram-20120701-by-volumes{0}.txt", sinceyear);
                    //outputFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701-allyears-by-volumes.txt";
                }

                tally.ReadFile();
                tally.Close();
                tally.OutputToFile(outputFile);

            } else if (verb == "tally-epithets") {

                //TODO: set this in options and make optional (can be set to null to skip this step)
                //string speciesFile = @"D:\Dropbox\latin2-more\beastierank\output\all species and synonyms CoL2014.csv";

                // epithtet counts for wiktionary 

                // (original meaning of "wikilist-species" is the same as "tally-species" with -w)

                var suboptions = options.WikilistSpecies;
                bool onlyNeedingWikiArticle = suboptions.onlyNeedingWikiArticle;

                var tally = new EpithetTally();
                tally.onlyCountMissingWiktionary = suboptions.onlyNeedingWikiArticle;

                if (!string.IsNullOrEmpty(suboptions.kingdom)) {
                    tally.kingdom = suboptions.kingdom;
                }


                if (suboptions.since != null) { //  && suboptions.since != 0
                    tally.startYear = (int)suboptions.since;
                } else {
                    tally.startYear = 1950;
                }

                string missing = tally.onlyCountMissingWiktionary ? "-missing" : "";
                string kingdom = string.IsNullOrEmpty(suboptions.kingdom) ? "" : "-" + suboptions.kingdom;

                string outputFile = string.Format(@"D:\ngrams\output-wiki\epithets-xx{0}-since{1}{2}.txt", kingdom, tally.startYear, missing);

                tally.ReadFile();
                tally.Close();
                tally.OutputEpithetCountsToFile(outputFile);

            } else if (verb == "wikipedia-pages-import") {
                //TODO: choose directory, and files, and download the files automatically too.

                //ImportWikipediaPagesAndRedirects();

                string dbName = "enwikipedia";

                string dir = @"D:\ngrams\datasets-wikipedia-en\";

                // doesn't work

                // attempts to create enwiktionary database instead of enwikipedia.

                string page_sql = dir + "enwiki-20150112-page.sql.gz";
                string redirect_sql = dir + "enwiki-20150112-redirect.sql.gz";

                // Unhandled Exception: OutOfMemoryException.
                //WiktionaryDatabase.ImportSmallDatabaseFile(pageSql);
                //WiktionaryDatabase.ImportSmallDatabaseFile(redirect_sql);

                var db = new CatalogueOfLifeDatabase();
                db.Connection(); // start the database

                db.RunMySqlImport(page_sql, dbName);
                db.RunMySqlImport(redirect_sql, dbName);

            } else if (verb == "percent-complete") {
                // percent-complete
                var suboptions = options.PercentComplete;
                var tally = new PercentDone();
                tally.ReadFile();
                tally.Close();
                tally.PrintResults();

            } else if (verb == "wikipedia-redlist") {
                var redlist = new RedlistCSV();
                //redlist.ReadCSV(); // automatic now
                redlist.OutputReports();

            } else if (verb == "redlist-tree") {
                // list child nodes of an iucn taxa
                var suboptions = options.RedlistNodes;

                if (string.IsNullOrWhiteSpace(suboptions.taxon)) {
                    Console.Error.WriteLine("No taxon speecified. e.g.");
                    Console.Error.WriteLine("beastie redlist-tree -t Squamata");
                    return;
                }

                var redlist = new RedlistCSV();
                //redlist.ReadCSV();
                redlist.ListChildNodes(suboptions.taxon, suboptions.useRules, suboptions.depth);

            } else if (verb == "desc") {
                var suboptions = options.Descendants;
                string epithet = suboptions.epithet;

                new DescendantsRequest(epithet, suboptions.rigorous);

            } else if (verb == "get-gni") {

                var gni = new GNIDownloader();
                //gni.Test();
                gni.ReadUrisAAAZZZ();

            } else if (verb == "ml") {

                new WordClassifier().BinomialTrainingTest();
                
            } else if (verb == "jobs") {

                var suboptions = options.Jobs;
                beastie.beastieDB.ImportJob.ListJobs(suboptions.id);

            } else if (verb == "jobs-clean") {

                var suboptions = options.JobsClean;
                beastie.beastieDB.ImportJob.CleanJobs(null, suboptions.markonly, suboptions.keepMainEntry);

            } else if (verb == "job-del") {
                var suboptions = options.JobDelete;
                if (suboptions.id == null) {
                    Console.Error.WriteLine("job id required. e.g. --id 123");
                    return;
                }
                    
                beastie.beastieDB.ImportJob.DeleteJob((long)suboptions.id, suboptions.markonly, suboptions.keepMainEntry);

            } else if (verb == "job-rerun") {

            } else if (verb == "wordvec") {

                var suboptions = options.WordVec;

                //new TestAnnoy().AnnoyTests();

                //new TestPretrainedVocab().TestAll();

                new TestPerformance().Test();

                //new TestPretrainedVocab().TestSkipGramPrefix();
                //new TestPretrainedVocab().DimensionExamples();

                //new beastie.pagebuilder.PageBuilder().BuildPages();

                //new NgramDependsSummarizer().CreateNgramDependTable();

                //new DictionaryCreator().ImportWordnetLemmas(suboptions.force); // ok now

                //new DictionaryCreator().CreateWordListsFromVocabs(suboptions.force); // note: uses a lot of ram

                //new DictionaryCreator().CreateSimiliarWordsLists(suboptions.force); // this is good. and it can resume.



            } else if (verb == "gensimIndex") {
                var suboptions = options.GensimIndex;

                new TestPretrainedVocab().TestGensimBridge();


            } else if (verb == "dev") {

                new TriangleInequalityTest().Test();

                //new WordVectorTest().Test(false);
                //new WordVectorTest().Test(true);

                //new WordCluster().TestClusterizing();

                //new ClusterTester().Test();
                /*
				new FixCatEponyms().PrintList();
                */

                /*
				var merger = new GNIMerger();
				merger.OutputCsv();
				*/

                //var merger = new GNICsv();
                //merger.ListBadUnicode();
                //merger.ListControlCharacter();
                //merger.ListSuspiciousWords();
                //merger.TestEscaping();
                //merger.TestDoubleEscaping();
                //merger.TestForChujoisms();
                //merger.RepairablePercent();
                //merger.UnknownPlacementPercent();

                /*
				string bad = "Agave √ó cavanillesii D.Guillot & P.Van der Meer";
				GNIStrings.DetectEncodingPretty("RïøΩmer", "Römer", false);
				GNIStrings.DetectEncodingPretty("RamÆrez", null, false);
				GNIStrings.DetectEncodingPretty("AntonÌn", null, false);
				GNIStrings.DetectEncodingPretty("Mƒll", null, false);
				GNIStrings.DetectEncodingPretty("DugÃ‹s", null, false);
				GNIStrings.DetectEncodingPretty("DugËs", null, false);
				GNIStrings.DetectEncodingPretty("MoÎnne", null, false);
				GNIStrings.DetectEncodingPretty("HollÛs", null, false);
				GNIStrings.DetectEncodingPretty("WichanskÞ", null, false);
				GNIStrings.DetectEncodingPretty("BoubÉÉ", null, false);
				GNIStrings.DetectEncodingPretty("BÜrner", null, false);
				GNIStrings.DetectEncodingPretty("DÜll", null, false);
				GNIStrings.DetectEncodingPretty("LacepËde", null, false);
				GNIStrings.DetectEncodingPretty("WichanskÃ", null, false);
				GNIStrings.DetectEncodingPretty("BÃ\u009crner", null, false);
				GNIStrings.DetectEncodingPretty("MarÃa", null, false);
				GNIStrings.DetectEncodingPretty("MendonÁa", null, false);
				*/

                /*
				//https://stackoverflow.com/questions/10484833/detecting-bad-utf-8-encoding-list-of-bad-characters-to-sniff
				string baderic = "as Ã‰ric";
				string shouldbe = "as Éric";
				Console.WriteLine("Bad:       " + baderic);
				Console.WriteLine("Fixed:     " + baderic.FixUTF());
				Console.WriteLine("Fixed:     " + baderic.FixUTF(Encoding.GetEncoding(1252)));
				Console.WriteLine("Fixed:     " + baderic.FixUTF(Encoding.GetEncoding("Windows-1252")));
				Console.WriteLine("Should be: " + shouldbe);

				foreach (var enc in Encoding.GetEncodings()) {
					if (baderic.FixUTF(enc.GetEncoding()) == shouldbe) {
							Console.WriteLine("encoding fixes it: " + enc.Name + " -- " + enc.CodePage + " -- " + enc.DisplayName);
					}
				}
				*/

                /*
				var merger = new GNIMerger();
				merger.ListMultilineRecords();
				*/

                /*
				var merger = new GNIMerger();
				merger.TestMaxId();
				*/


                /*
				new RedlistCSV().TallyThreatenedEpithets();
*/
                /*
				new LuaStyleStemmer().Test();
				*/

                /*
				new CenturyReader().Test();
				*/


                /*
				DotNetWikiBot.Bot.cacheDir = @"C:\Cache"; //TODO: move this somewhere and/or make configurable

				BeastieBot.TestTaxon("Blue whale");
				BeastieBot.TestTaxon("Pterobalaena gigas"); // blue whale synonym
				BeastieBot.TestTaxon("Engaeus granulatus"); // redirects to Genus page
				BeastieBot.TestTaxon("Kapcypridopsis barnardi");
				BeastieBot.TestTaxon("Bedotia sp. nov. 'Sambava'");
				BeastieBot.TestTaxon("Giant salmon carp");
				BeastieBot.TestTaxon("Widemouth gambusia");
				BeastieBot.TestTaxon("Trichonis blenny");
				BeastieBot.TestTaxon("Lancer dragonet");
				BeastieBot.TestTaxon("Konye");
				*/


                /*
				var xowa = new XowaDB();
				Console.WriteLine(xowa.ReadPageText());
				Console.WriteLine(xowa.ReadPage("pengo"));

				var cat = xowa.ReadEntry("cat");
				var catLangs = cat.Sections().Keys.JoinStrings(", ");
				Console.WriteLine("languages of cat: " + catLangs);

				var wikt = WiktionaryBot.Instance();
				Console.WriteLine("Latin/Translingual Exists: {0}", wikt.ExistsMulLa("cat")); // false (via db)
				Console.WriteLine("Latin/Translingual Exists: {0}", wikt.ExistsMulLa("cattus")); // true (via db)
				Console.WriteLine("Latin/Translingual Exists: {0}", wikt.ExistsMulLa("telonium")); // true (required log in)
				Console.WriteLine("Latin/Translingual Exists: {0}", wikt.ExistsMulLa("doallat")); //  false (required log in)
				*/

                /*
				LatinStemBall.Test();
				*/

                /*
				string sp0 = "Haldina cordifolia";
				var details0 = new SpeciesDetails(sp0);
				details0.Query();
				Console.WriteLine(sp0 + ": " + details0.NeedsEnWikiArticle());

				string sp1 = "Solanum tuberosum"; // should definitely work
				var details1 = new SpeciesDetails(sp1);
				details1.Query();
				Console.WriteLine(details1.species + " - " + details1.family + " - " + details1.kingdom);
				Console.WriteLine(details1.status);

				string sp = "Helix inconvicta"; // "Triboniophorus krefftii"; // non_accepted_species 
				var details = new SpeciesDetails(sp);
				details.Query();
				Console.WriteLine(details.species);
				Console.WriteLine(details.status);
				Console.WriteLine(" = " + details.AcceptedSpeciesDetails().species);
				*/

                // text xowa database reading (works)
                //new XowaDB().ReadPageText();

                /*
				var x = new XowaWeb();
				x.StartWebService();
				x.EnWiktionaryPage("cat");
				x.EnWiktionaryPage("ዶሮ"); // Amharic
				x.EnWiktionaryPage("pigeon");
				x.EnWiktionaryPage("flea");
				x.KillWebService();
				*/


            } else if (verb == "help") {
				Console.Error.WriteLine("this never gets called");
				Console.Error.Write(options.GetUsage(verb));
			}


            //non-verb way of using options
            //if (CommandLine.Parser.Default.ParseArguments(args, options)) {
            // Values are available here
            //if (options.Verbose) Console.WriteLine("Filename: {0}", options.InputFile);
            //}

            // D:\Dropbox\latin2-more\beastierank\bin\Debug\beastie.exe

            // "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\bin\mysqld"

            //Console.WriteLine ("Hello World!");

            //Uncomment one of these:     //TODO: have an interface/arguments or separate programs to choose

            //FliRegexTest();

            //Beastie();
            //BuildSpeciesTable();

            //Warning: BuildNgramDatabase is very slow (12+ hours) and add counts to old records (does not replace old, so need to drop ng_lemmas table first)
            //BuildNgramDatabase();

            // wiktionary:
            // *** now replaced with: WiktionaryDatabaseUtilities.BuildEverything()
            ////BuildWiktionaryLanguageList();
            //ImportWiktionaryDatabase();
            //BuildLanguageCategoryTable();
            //CreateWiktLemmasTable();

            //CreateScannoIndexWiktionaryAndNgrams(); // first: DROP TABLE pengo.stem_index;  (or truncate)

            //RankText();	

            //PrintStemmerExamples();

            //WordLangs("croissant");
            //WordLangs("dog");

            //ProcessWiktionaryEntries();

            //DownloadAndProcessTwoGramSpecies();

        }

        static void ProcessWiktionaryEntries() {
			string path = FileConfig.datadir + @"datasets-wiki\";
			string file = path + @"enwiktionary-20140328-pages-articles.xml.bz2";
			//string file = path + @"enwiktionary-20140206-pages-meta-current.xml.bz2";

			WiktionaryXMLEntries entries = new WiktionaryXMLEntries(file);
			//entries.PrintXml();
			//entries.Process();
			entries.TemplateUsageStats();

		}
		static void FliRegexTest() {
			Lemma.FliRegexTest();
		}

		static void CreateScannoIndexWiktionaryAndNgrams() {
			NgramDatabase ngramdb = new NgramDatabase();
			ngramdb.CreateTables();
			ngramdb.CreateScannoIndexWiktionary();
			ngramdb.CreateScannoIndexNgram();
		}
		static void WordLangs(string word) {
			WiktionaryData wiktionaryData = WiktionaryData.Instance();
            var wiktUtil = new WiktionaryDatabaseUtilities();

            string[] langCodes = wiktUtil.LanguagesOfTerm(word);
            string codes = langCodes.JoinStrings(", ");
			string langs = langCodes.Select(w => wiktionaryData.codeIndex[w].canonicalName).JoinStrings(", ");
			//Console.WriteLine("{0}", langCodes); // en, fi, fr, sv)
			Console.WriteLine("{0}: {1}", word, langs); // English, Finnish, French, Swedish
		}

		static void BuildSpeciesTable() {
			CatalogueOfLifeDatabase col = new CatalogueOfLifeDatabase();
			col.BuildSpeciesTable();

		}
		static void BuildWiktionaryLanguageList() {
			new WiktionaryData();
		}

		static void ImportWiktionaryDatabase() {
			//TODO: replace with newer method used by wikipedia-pages-import

			string dir = FileConfig.datadir + @"datasets-wiktionary-en\";

            // not sql: "enwiktionary-20140222-all-titles-in-ns0.gz"

            //TODO: FIXME: ImportSmallDatabaseFile() moved to MyDatabase
            //WiktionaryDatabaseUtilities.ImportSmallDatabaseFile(dir + "enwiktionary-20140328-site_stats.sql.gz");
			//WiktionaryDatabaseUtilities.ImportSmallDatabaseFile(dir + "enwiktionary-20140328-page.sql.gz");
			//WiktionaryDatabaseUtilities.ImportSmallDatabaseFile(dir + "enwiktionary-20140328-category.sql.gz");

			////WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140328-categorylinks.sql.gz"); // fails with OutOfMemoryError. See command line arguments in CatalogueOfLifeDatabase.cs

			// https://www.mediawiki.org/wiki/Manual:Categorylinks_table
			//SELECT CONVERT(cl_to USING utf8), CONVERT(cl_sortkey USING utf8), CONVERT(cl_collation USING utf8) FROM enwiktionary.categorylinks LIMIT 0,100000;
			//WiktionaryDatabase.ImportDatabaseFile(@"D:\ngrams\datasets-wiki\enwiktionary-20140206-categorylinks.sql.gz");

			//SELECT convert(page_title using utf8) as title, page.* FROM enwiktionary.page WHERE page_namespace = 0 and page_is_redirect = 0;
		}


		static void PrintStemmerExamples() {
			StemmerExamples.PrintStemmerExamples();
		}

		static void RankText() {
			string textDir = FileConfig.datadir + @"books\";
			string text = textDir + "The_Satanic_Verses.txt";
			//string text = textDir + "ALICES_ADVENTURES_IN_WONDERLAND.txt";
			//string text = textDir + "Baba-Yaga_and_Vasilisa_the_Fair.txt";
			//string text = textDir + "GULLIVERS_TRAVELS.txt";
			//string text = textDir + "Frankenstein.txt";
			//string text = textDir + "Wuthering_Heights.txt";
			//string text = textDir + "The_Iliad_of_Homer.txt";

			string massagedDir = FileConfig.datadir + @"massaged\";
			string massagedData = massagedDir + "all-fiction.txt";
			string massagedDataStems = massagedDir + "all-fiction-stems.txt";

			NgramRanker ranker = new NgramRanker();
			//ranker.SetMassagedData(massagedData);
			ranker.SetMassagedDataStems(massagedDataStems);
			ranker.RankText(text);
			ranker.PrintTop();
		}

		static void BuildNgramDatabase() { // was: BuildMassagedNgramData()
			//WARNING: counts will be doubled if run twice

			//TODO: output is to d:\ngrams\massaged\all-fiction.txt

			//NgramReader ngramReader = new NgramReader("D:\\ngrams\\googlebooks-eng-all-1gram-20120701-a.gz");
			//NgramReader ngramReader = new NgramReader();
			NgramDbReader ngramReader = new NgramDbReader("eng-fiction-all-1950+", 1950);
			string dir = FileConfig.datadir + @"datasets\";
			string outDir = FileConfig.datadir + @"massaged\";
			string outputFileLemmas = outDir + "all-fiction.txt";
			string outputFileStems = outDir + "all-fiction-stems.txt";

            //ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-a.gz");
            //ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-b.gz");

            //string template = ""; // a-z
            foreach (var f in NgramFileIterator.AZ("googlebooks-eng-fiction-all-1gram-20120701-{0}.gz")) {
                ngramReader.ReadFile(f);
            }

			//TODO: "punctuation" and "other"

			//for NgramReader, not NgramDbReader
			//ngramReader.PrintAllLemmas(outputFileLemmas);
			//ngramReader.PrintAllStems(outputFileStems);
		}

		static void Beastie() {
			SpeciesSet colSpecies = new SpeciesSet();
			colSpecies.ReadCsv(@"C:\Dropbox\latin2-more\beastierank\data\all-species.csv");
			StemGroups groups = colSpecies.GroupEpithetStems();
			groups.PrintGroups();
			Console.WriteLine();
			Console.WriteLine();
			//groups.PrintGroup("bulbophylli");
			//groups.PrintGroup("rex");
			
			groups.GroupStats();
			
			//TODO:
			//SpeciesReader wikipediaSpecies = new SpeciesReader("D:\\Dropbox\\latin2-more\\beastierank\\data\\all-species.csv").ReadCsv();

		}
	}


}