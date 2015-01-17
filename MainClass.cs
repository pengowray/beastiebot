using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;

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
				string speciesFile = suboptions.speciesList;
				if (speciesFile == null || speciesFile == "") {
					speciesFile = @"D:\Dropbox\latin2-more\beastierank\output\all species and synonyms CoL2014.csv";
				}

				var filter = new NgramSpeciesFilter();
				filter.LoadSpeciesSet(speciesFile);

				//TODO: move this to another verb
				Console.WriteLine("First characters: ");
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
			} else if (verb == "tally-species") {
				var suboptions = options.TallySpecies;
				string outputFile = suboptions.outputFile;
				string speciesNgramFile = suboptions.speciesNgramFile; // TODO: rename to SpeciesFile, as specieslist is something else in filter-2gram-species

				if (speciesNgramFile == null || speciesNgramFile == "") {
					speciesNgramFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701.txt";
				}


				if (string.IsNullOrEmpty(outputFile)) {
					// default file name
					if (suboptions.wikiStyleOutput) {
						if (string.IsNullOrEmpty(suboptions.kingdom)) {
							outputFile = @"D:\ngrams\D:\ngrams\output-wiki\species-wiki.txt";
						} else {
							outputFile = @"D:\ngrams\D:\ngrams\output-wiki\species-wiki-"+ suboptions.kingdom + @".txt";
						}
					} else {	
						outputFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701-post-1950-by-volumes.txt";
						//outputFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701-allyears-by-volumes.txt";
					}
				}

				var tally = new NgramSpeciesTally();

				tally.startYear = 1950; // TODO: set year from parameter (0 for all years)

				if (!string.IsNullOrEmpty(suboptions.kingdom)) {
					tally.kingdom = suboptions.kingdom;
				}

				tally.ReadFile(speciesNgramFile);
				tally.Close();
				if (suboptions.wikiStyleOutput) { // -w
					tally.WikiListToFile(outputFile);
				} else {
					tally.OutputToFile(outputFile);
				}

			} else if (verb == "wikilist-species") {
				//meh. just use above with -w
				var suboptions = options.WikilistSpecies;

				string speciesFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701-post-1950-by-volumes.txt";

			} else if (verb == "dev") {

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
			////BuildWiktionaryLanguageList();
			//ImportWiktionaryDatabase();
			//BuildLanguageCategoryTable(); // first: TRUNCATE pengo.wikt_category_languages;
			//CreateWiktLemmasTable(); //TODO: rename lemmas to words or terms

			//CreateScannoIndexWiktionaryAndNgrams(); // first: DROP TABLE pengo.stem_index;  (or truncate)

			//RankText();	

			//PrintStemmerExamples();

			//WordLangs("croissant");
			//WordLangs("dog");

			//ProcessWiktionaryEntries();

			//DownloadAndProcessTwoGramSpecies();

		}

		static void ProcessWiktionaryEntries() {
			string path = @"D:\ngrams\datasets-wiki\";
			string file = path + @"enwiktionary-20140328-pages-articles.xml.bz2";
			//string file = path + @"enwiktionary-20140206-pages-meta-current.xml.bz2";

			WiktionaryEntries entries = new WiktionaryEntries(file);
			//entries.PrintXml();
			//entries.Process();
			entries.TemplateUsageStats();

		}
		static void FliRegexTest() {
			Lemma.FliRegexTest();
		}

		static void CreateWiktLemmasTable() {
			WiktionaryDatabase.CreateWiktLemmasTable();
		}

		static void CreateScannoIndexWiktionaryAndNgrams() {
			NgramDatabase ngramdb = new NgramDatabase();
			ngramdb.CreateTables();
			ngramdb.CreateScannoIndexWiktionary();
			ngramdb.CreateScannoIndexNgram();
		}
		static void WordLangs(string word) {
			WiktionaryData wiktionaryData = WiktionaryData.Instance();

			string langCodes = WiktionaryDatabase.LanguagesOfTerm(word).JoinStrings(", ");
			string langs = WiktionaryDatabase.LanguagesOfTerm(word).Select(w => wiktionaryData.codeIndex[w].canonicalName).JoinStrings(", ");
			//Console.WriteLine("{0}", lang                                                                                                                              Codes); // en, fi, fr, sv)
			Console.WriteLine("{0}", langs); // English, Finnish, French, Swedish
		}

		static void BuildSpeciesTable() {
			CatalogueOfLifeDatabase col = new CatalogueOfLifeDatabase();
			col.BuildSpeciesTable();

		}
		static void BuildWiktionaryLanguageList() {
			new WiktionaryData();
		}

		static void ImportWiktionaryDatabase() {

			string dir = @"D:\ngrams\datasets-wiki\";

			// not sql: "enwiktionary-20140222-all-titles-in-ns0.gz"
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140328-site_stats.sql.gz");
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140328-page.sql.gz");
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140328-category.sql.gz");
			//WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140328-categorylinks.sql.gz"); // fails with OutOfMemoryError. See command line arguments in CatalogueOfLifeDatabase.cs

			// https://www.mediawiki.org/wiki/Manual:Categorylinks_table
			//SELECT CONVERT(cl_to USING utf8), CONVERT(cl_sortkey USING utf8), CONVERT(cl_collation USING utf8) FROM enwiktionary.categorylinks LIMIT 0,100000;
			//WiktionaryDatabase.ImportDatabaseFile(@"D:\ngrams\datasets-wiki\enwiktionary-20140206-categorylinks.sql.gz");

			//SELECT convert(page_title using utf8) as title, page.* FROM enwiktionary.page WHERE page_namespace = 0 and page_is_redirect = 0;
		}

		static void BuildLanguageCategoryTable() {
			WiktionaryDatabase.BuildLanguageCategoryTable();
		}

		static void PrintStemmerExamples() {
			StemmerExamples.PrintStemmerExamples();
		}

		static void RankText() {
			string textDir = @"D:\ngrams\books\";
			string text = textDir + "The_Satanic_Verses.txt";
			//string text = textDir + "ALICES_ADVENTURES_IN_WONDERLAND.txt";
			//string text = textDir + "Baba-Yaga_and_Vasilisa_the_Fair.txt";
			//string text = textDir + "GULLIVERS_TRAVELS.txt";
			//string text = textDir + "Frankenstein.txt";
			//string text = textDir + "Wuthering_Heights.txt";
			//string text = textDir + "The_Iliad_of_Homer.txt";

			string massagedDir = @"D:\ngrams\massaged\";
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
			string dir = @"D:\ngrams\datasets\";
			string outDir = @"D:\ngrams\massaged\";
			string outputFileLemmas = outDir + "all-fiction.txt";
			string outputFileStems = outDir + "all-fiction-stems.txt";

			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-a.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-b.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-c.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-d.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-e.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-f.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-g.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-h.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-i.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-j.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-k.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-l.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-m.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-n.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-o.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-p.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-q.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-r.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-s.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-t.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-u.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-v.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-w.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-x.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-y.gz");
			ngramReader.ReadFile(dir + "googlebooks-eng-fiction-all-1gram-20120701-z.gz");
			//TODO: "punctuation" and "other"

			//for NgramReader, not NgramDbReader
			//ngramReader.PrintAllLemmas(outputFileLemmas);
			//ngramReader.PrintAllStems(outputFileStems);
		}

		static void Beastie() {
			SpeciesSet colSpecies = new SpeciesSet();
			colSpecies.ReadCsv(@"D:\Dropbox\latin2-more\beastierank\data\all-species.csv");
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