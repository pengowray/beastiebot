using System;

namespace beastie
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.WriteLine ("Hello World!");

			SpeciesSet colSpecies = new SpeciesSet("D:\\Dropbox\\latin2-more\\beastierank\\data\\all-species.csv");
			colSpecies.ReadCsv();
			StemGroups groups = colSpecies.GroupEpithetStems();
			groups.PrintGroups();
			Console.WriteLine();
			Console.WriteLine();
			groups.PrintGroup("bulbophylli");
			//groups.PrintGroup("rex");

			groups.GroupStats();

			//TODO:
			//SpeciesReader wikipediaSpecies = new SpeciesReader("D:\\Dropbox\\latin2-more\\beastierank\\data\\all-species.csv").ReadCsv();


		}
	}


}
