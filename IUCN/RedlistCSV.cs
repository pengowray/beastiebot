using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class RedlistCSV
	{

		TaxonNode topNode;
		Dictionary<string,string> possiblyExtinct = new Dictionary<string, string>();

		string outputFileName = @"D:\ngrams\output-wiki\iucn-list-{0}.txt";



		public RedlistCSV() {
		}

		public void ReadPossiblyExtinct() {
			// read file which is a bit of a mess because it's a copy/paste from a pdf
			string pefile = FileConfig.Instance().iucnPossiblyExtinctFile;
			using (var infile = new StreamReader(pefile , Encoding.UTF8, true)) {
				string line;
				while ((line = infile.ReadLine()) != null) {

					string special = null;
					if (line.Contains(" CR(PE)")) {
						special = " CR(PE)";
					} else if (line.Contains(" CR(PEW)")) {
						special = " CR(PEW)";
					} else {
						continue;
					}

					string left = line.Substring(0, line.IndexOf(special));
					// Pantanodon sp. nov. 'Manombo' CR(PE) 2004 1997
					// Rhizopsammia wellingtoni Wellington's Solitary Coral CR(PE) 2007 2000
					// Bulimulus sp. nov. 'tuideroyi' CR(PE) 2003 ?
					// Conturbatia crenata CR(PE) 2006 2000

					string bitri;

					var matches = Regex.Match(left, "([A-Z].*?) [A-Z]");
					if (matches.Captures.Count == 0) {
						bitri = left;
					} else {
						bitri = matches.Groups[1].Captures[0].Value;
					}

					//Console.WriteLine("bitri: [" + bitri + "]" + special);
					//if (match.Success  match.Captures
					possiblyExtinct[bitri.ToLowerInvariant()] = special.Trim();
				}

			}
		}

		public void ReadCSV() {

			ReadPossiblyExtinct();

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

			//string iucnRedListFile = @"D:\ngrams\datasets-iucn\2014.3\export-56959.csv";
			//string iucnRedListFileName = @"D:\ngrams\datasets-iucn\2014.3\2015-02-09_Everything-but-regional-export-57234.csv\export-57234.csv";
			string iucnRedListFileName = FileConfig.Instance().iucnRedListFile;

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
				topNode = new TaxonNode();
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


					/*
					// example weirdness:
					// puget sound-georgia basin: nimpkish, mackenzie r
					// hecate strait-q.c. sound: kitimat to kitlope r
					if (!string.IsNullOrEmpty(csv[12])) { // quick survey
						string stockpop = csv[12].ToLowerInvariant();
						if (!stockpop.Contains("subpopulation") && !stockpop.Contains("stock")) {
							//Console.WriteLine("weird: " + csv[12]);
						}
					}
					*/

					//TODO: a little inefficent to ExtractBitri() now just to do it again later, but whatever
					string basicName = details.ExtractBitri().BasicName().ToLowerInvariant();
					if (possiblyExtinct.ContainsKey(basicName)) {
						details.Add("special status", possiblyExtinct[basicName]);
					}

					topNode.Add(details);
					//if (possiblyExtinct.ContainsKey(

					count++;
				}

				CreateList("Mammalia", "CR");
				CreateList("Mammalia", null);
				//CreateList("Testudines", "CR");
				CreateList("Testudines", null);

				Console.WriteLine("Done. Entry count: {0}", count);
			}

		}

		void CreateList(string category, string status = null) {
			//var subNode = topNode.FindChildDeep("Animalia");
			var subNode = topNode.FindChildDeep(category); // e.g. Mammalia
			//var subNode = topNode.FindChildDeep("CHIROPTERA"); // works
			//var subNode = topNode.FindChildDeep("Fish");
			//topNode.PrettyPrint(output);
			if (subNode == null) {
				Console.Error.WriteLine("Failed to find subnode for category: " + category);
			} else {
				string catStr = (category == null ? "" : "-" + category.TitleCaseOneWord());
				string statusStr = (status == null ? "" : "-" + status);

				string filename = string.Format(outputFileName, catStr + statusStr);
				StreamWriter output = new StreamWriter(filename, false, Encoding.UTF8);
				using (output) {
					subNode.PrettyPrint(output, status, 1);
				}
			}

		}
	}
}

