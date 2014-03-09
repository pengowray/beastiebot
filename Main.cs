using System;
using System.Linq;

namespace beastie
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// D:\Dropbox\latin2-more\beastierank\bin\Debug\beastie.exe

			// "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\bin\mysqld"

			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			//Console.WriteLine ("Hello World!");

			//Uncomment one of these:     //TODO: have an interface/arguments or separate programs to choose


			//Beastie();
			//BuildSpeciesTable();

			//Warning: very slow (many hours) and add counts to old records (does not replace old, so need to delete table first)
			//BuildNgramDatabase();

			//RankText();	

			//PrintStemmerExamples();

			//ImportWiktionaryDatabase();

			////BuildWiktionaryLanguageList();

			//BuildLanguageCategoryTable();

			//WordLangs("croissant");
			//WordLangs("dog");


		}

		static void WordLangs(string word) {
			WiktionaryData wiktionaryData = WiktionaryData.Instance();

			string langCodes = WiktionaryDatabase.LanguagesOfTerm(word).JoinStrings(", ");
			string langs = WiktionaryDatabase.LanguagesOfTerm(word).Select(w => wiktionaryData.codeIndex[w].canonicalName).JoinStrings(", ");
			Console.WriteLine("{0}", langCodes); // en, fi, fr, sv
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
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140222-site_stats.sql.gz");
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140222-page.sql.gz");
			WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140222-category.sql.gz");
			//WiktionaryDatabase.ImportDatabaseFile(dir + "enwiktionary-20140222-categorylinks.sql.gz"); // fails with OutOfMemoryError. See command line arguments in CatalogueOfLifeDatabase.cs

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
			SpeciesSet colSpecies = new SpeciesSet(@"D:\Dropbox\latin2-more\beastierank\data\all-species.csv");
			colSpecies.ReadCsv();
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
