using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class RedlistCSV
	{
		public RedlistCSV() {

		}

		public void ReadCSV() {
			/*
			string test1 = "Tarsius tumpara"; // Siau Island tarsier
			string test2 = "Tarsiidae"; // Tarsier
			string test3 = "Animalia"; // Animal

			BeastieBot.Instance().GetPage(test1, false).DebugPrint();
			BeastieBot.Instance().GetPage(test2, false).DebugPrint();
			BeastieBot.Instance().GetPage(test3, false).DebugPrint();
			BeastieBot.Instance().GetPage("Lion", false).DebugPrint();
			*/

			string status_filter = "CR";
			string outputFileName = @"D:\ngrams\output-wiki\iucn-critically-endangered.txt";

			//string iucnRedListFile = @"D:\ngrams\datasets-iucn\2014.3\export-56959.csv";
			string iucnRedListFileName = @"D:\ngrams\datasets-iucn\2014.3\2015-02-09_Everything-but-regional-export-57234.csv\export-57234.csv";
			using (var infile = new StreamReader(iucnRedListFileName, Encoding.GetEncoding(1252), true)) { // Windows-1252 encoding, not UTF-8. (e.g. for "Galápagos")
				CsvReader csv = new CsvReader(infile, true);

				//Species ID,Kingdom,Phylum,Class,Order,Family,Genus,Species,Authority,Infraspecific rank,Infraspecific name,Infraspecific authority,Stock/subpopulation,Synonyms,Common names (Eng),Common names (Fre),Common names (Spa),Red List status,Red List criteria,Red List criteria version,Year assessed,Population trend,Petitioned
				//3,ANIMALIA,MOLLUSCA,GASTROPODA,STYLOMMATOPHORA,ENDODONTIDAE,Aaadonta,angaurana,"Solem, 1976",,,,,"",,,,CR,B1ab(iii)+2ab(iii),3.1,2012,unknown,N

				string[] headers = csv.GetFieldHeaders();
				if (csv.FieldCount < 23) {
					//TODO: if one field, then treat as space-separated (using first space)
					throw new Exception(string.Format("ReadCSV() found wrong number of fields. Expected 23 or more. Found: {0}", headers.Length));
				}

				var rules = new TaxonDisplayRules();
				rules.Compile();

				List<TaxonDetails> detailList = new List<TaxonDetails>();
				TaxonNode topNode = new TaxonNode();
				topNode.rules = rules;
				topNode.name = "top";
				topNode.rank = "top";
				int count = 0;

				while (csv.ReadNextRecord()) {
					string speciesId = csv[0]; // "Species ID"];

					// "kingdom","phylum","class","order","family","genus","species"

					var details = new TaxonDetails();

					details.AddFromTop("kingdom", csv[1]);
					details.AddFromTop("phylum", csv[2]);
					details.AddFromTop("class", csv[3]);
					details.AddFromTop("order", csv[4]);
					details.AddFromTop("family", csv[5]);
					details.AddFromTop("genus", csv[6]);
					details.AddFromTop("species", csv[7]);
					details.Add("authority", csv[8]);
					details.Add("infraspecific rank", csv[9]);
					details.AddFromTop("infraspecific name", csv[10]);
					details.Add("Infraspecific authority", csv[11]);
					details.AddFromTop("stock/subpopulation", csv[12]);
					details.Add("synonyms", csv[13]);
					details.Add("common names (eng)", csv[14]);
					details.Add("common names (fre)", csv[15]);
					details.Add("common names (spa)", csv[16]);
					details.Add("red list status", csv[17]);
					//Red List criteria version,
					//Year assessed,
					//Population trend
					//Petitioned
					//detailList.Add(detailList);

					//Console.WriteLine("{0}", details.FullSpeciesName());
					topNode.Add(details);

					count++;
				}

				//var subNode = topNode.FindChildDeep("Animalia");
				var subNode = topNode.FindChildDeep("MAMMALIA"); // Mammalia
				//var subNode = topNode.FindChildDeep("CHIROPTERA"); // works
				//var subNode = topNode.FindChildDeep("Fish");
				//topNode.PrettyPrint(output);
				if (subNode == null) {
					Console.Error.WriteLine("Failed to find top node");
				} else {
					StreamWriter output = new StreamWriter(outputFileName, false, Encoding.UTF8);
					subNode.PrettyPrint(output, status_filter);
					output.Close();
				}
				Console.WriteLine("Done. Entry count: {0}", count);

			}

		}
	}
}

