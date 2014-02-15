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

			//BuildMassagedNgramData();

			RankText();

			StemTest();
		}
		static void StemTest() {
			//string word = "Seated";
			string word = "Listlessness";

			SF.Snowball.Ext.EnglishStemmer eng = new SF.Snowball.Ext.EnglishStemmer();
			eng.SetCurrent(word);
			eng.Stem();
			Console.WriteLine(eng.GetCurrent()); //returns Seat

		}
		static void RankText() {
			string textDir = "D:\\ngrams\\books\\";
			string text = textDir + "The_Satanic_Verses.txt";
			//string text = textDir + "ALICES_ADVENTURES_IN_WONDERLAND.txt";
			//string text = textDir + "Baba-Yaga_and_Vasilisa_the_Fair.txt";
			//string text = textDir + "GULLIVERS_TRAVELS.txt";

			string massagedDir = "D:\\ngrams\\massaged\\";
			string massagedData = massagedDir + "all-fiction.txt";

			NgramRanker ranker = new NgramRanker();
			ranker.SetMassagedData(massagedData);
			ranker.RankText(text);
			ranker.PrintTop();
		}

		static void BuildMassagedNgramData() {
			//TODO: output is to d:\ngrams\massaged\all-fiction.txt

			//NgramReader ngramReader = new NgramReader("D:\\ngrams\\googlebooks-eng-all-1gram-20120701-a.gz");
			NgramReader ngramReader = new NgramReader();
			string dir = "D:\\ngrams\\datasets\\";
			string outDir = "D:\\ngrams\\massaged\\";
			string outputFile = outDir + "all-fiction.txt";

			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-a.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-b.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-c.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-d.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-e.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-f.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-g.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-h.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-i.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-j.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-k.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-l.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-m.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-n.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-o.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-p.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-q.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-r.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-s.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-t.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-u.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-v.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-w.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-x.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-y.gz");
			ngramReader.ReadIntoDatabase(dir + "googlebooks-eng-fiction-all-1gram-20120701-z.gz");
			//TODO: "punctuation" and "other"
			ngramReader.PrintAll(outputFile);
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
