using System;

namespace beastie
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// D:\Dropbox\latin2-more\beastierank\bin\Debug\beastie.exe

			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			//Console.WriteLine ("Hello World!");


			//Uncomment one of these:     //TODO: have an interface/arguments or separate programs to choose


			//Beastie();
			//BuildSpeciesTable();

			//BuildMassagedNgramData();

			//RankText();

			//PrintStemmerExamples();

			//BuildNgramDatabase();

			BuildWiktionaryLanguageList();
			BuildWordLanguageListFromWiktionaryCats();


		}

		static void BuildSpeciesTable() {
			CatalogueOfLifeDatabase col = new CatalogueOfLifeDatabase();
			col.BuildSpeciesTable();

		}
		static void BuildWiktionaryLanguageList() {
			new WiktionaryData();
		}

		static void BuildWordLanguageListFromWiktionaryCats() {

		}

		static void BuildNgramDatabase() {
			NgramDatabase ngram = new NgramDatabase();
			ngram.CreateTables();
		}

		static void PrintStemmerExamples() {
			StemmerExamples.PrintStemmerExamples();
		}

		static void RankText() {
			string textDir = @"D:\ngrams\books\";
			//string text = textDir + "The_Satanic_Verses.txt";
			//string text = textDir + "ALICES_ADVENTURES_IN_WONDERLAND.txt";
			//string text = textDir + "Baba-Yaga_and_Vasilisa_the_Fair.txt";
			//string text = textDir + "GULLIVERS_TRAVELS.txt";
			//string text = textDir + "Frankenstein.txt";
			string text = textDir + "Wuthering_Heights.txt";
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

		static void BuildMassagedNgramData() {
			//TODO: output is to d:\ngrams\massaged\all-fiction.txt

			//NgramReader ngramReader = new NgramReader("D:\\ngrams\\googlebooks-eng-all-1gram-20120701-a.gz");
			NgramReader ngramReader = new NgramReader();
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

			ngramReader.PrintAllLemmas(outputFileLemmas);
			ngramReader.PrintAllStems(outputFileStems);
		}

		static void Beastie() {
			SpeciesSet colSpecies = new SpeciesSet("D:\\Dropbox\\latin2-more\\beastierank\\data\\all-species.csv");
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
