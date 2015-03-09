using System;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//using CSharpParser.Utility;
using beastie;

namespace beastie {
	public class GniItem {
		public string id;
		public string name;
		public int int_id {
			get { return int.Parse(id); }
		}
	}

	public class GNICsv
	{
		public GNICsv() {
		}

		public virtual IEnumerable<GniItem> Records() {
			string fileName = FileConfig.Instance().GNISafeEscapedCSV;
			using (var infile = new StreamReader(fileName, Encoding.UTF8, true)) { // Windows-1252 encoding, not UTF-8. (e.g. for "Galápagos")
				int lineCount = 0;
				infile.ReadLine(); // headers
				lineCount++;
				string line;
				while ((line = infile.ReadLine()) != null) {
					lineCount++;
					var csv = line.Split(new char[] { ',' }, 2);
					var item = new GniItem();
					if (csv.Length != 2) {
						Console.Error.WriteLine("MalformedCsvException");
						Console.Error.WriteLine("line count: " + lineCount);
						Console.Error.WriteLine("last line: " + line);
					} else {
						/*
						if (!csv[1].EndsWith("\"")) {
							csv[1] += " " + infile.ReadLine();
							lineCount++;
							Console.WriteLine(lineCount + " two liner: " + csv[1]);
						}
						*/

						item.id = csv[0];
						item.name = csv[1].CsvUnescapeSafe();
						yield return item;
					}
				}
			}
		}

		// this probably works fine too, but not sure how it unescapes things
		public virtual IEnumerable<GniItem> RecordsCsvReader() {
			//long lineCount = 1;
			string fileName = FileConfig.Instance().GNISafeEscapedCSV;
			using (var infile = new StreamReader(fileName, Encoding.UTF8, true)) { // Windows-1252 encoding, not UTF-8. (e.g. for "Galápagos")
				CsvReader csv = new CsvReader(infile, true);

				string[] headers = csv.GetFieldHeaders();
				//csv.SupportsMultiline = true; // hopefully don't need it now that it's escaped

				while (csv.ReadNextRecord()) {
					var item = new GniItem();
					item.id = csv[0];
					item.name = csv[1].UnescapeCSharpLiteral();
					yield return item;
				}
			}
		}

		public void OutputCsv() {
			Console.WriteLine("id,name");
			foreach (var record in Records()) {
				string doubleEscaped = record.name.CsvEscapeSafe();
				Console.WriteLine("{0},{1}", record.id, doubleEscaped);
			}
		}

		public void TestDoubleEscaping() {
			foreach (var record in Records()) {
				//string escaped = record.name.EscapeToCSharpLiteral().CsvEscape(true);
				//string unescaped = escaped.CsvUnescape().UnescapeCSharpLiteral();
				string escaped = record.name.CsvEscapeSafe();
				string unescaped = escaped.CsvUnescapeSafe();
				if (unescaped != record.name) {
					Console.WriteLine(record.id + ": " + record.name);
					Console.WriteLine(record.id + ": " + unescaped);
				}
			}
			Console.WriteLine("Done.");
		}

		public void TestEscaping() {
			foreach (var record in Records()) {
				string escaped = record.name.EscapeToCSharpLiteral();
				string unescaped = escaped.UnescapeCSharpLiteral();
				if (unescaped != record.name) {
					Console.WriteLine(record.id + ": " + record.name);
					Console.WriteLine(record.id + ": " + unescaped);
				}
			}
			Console.WriteLine("Done.");
		}

		public void ListSuspiciousWords() {
			foreach (var record in Records()) {
				//string name = GNIStrings.RepairEncodingPlus(record.value);
				string name = GNIStrings.RepairCharChoices(GNIStrings.RepairEncoding(record.name));
				if (GNIStrings.IsSuspiciousCamel(name, true)) {
					Console.WriteLine(record.id + ": " + name);
				}

			}

		}

		public void ListControlCharacter() {
			foreach (var record in Records()) {

				// none
				if (record.name.Contains("\r")) {
					Console.WriteLine(@"\r: {0}: {1}", record.id, record.name);
				}


				// didn't catch: 17866200: Abirellus Ch&#x000FB;j&#x000F4; == Chûjô  //TODO

				//2 tabs:
				// 22063467 as C# literal: Lasionycta perplexella\tCrabo et Lafontaine, 2009
				// 12054467 as C# literal: Desmanthus illinoiensis\tillinoiensis (Michx.) MacMillan ex Rob. & Fern.

				//2x \u007F  (del control code)
				// 11390612: Aglaia rubiginosa (Hiern\u007F) Pannell
				// 10573352: Pleurotus ostreatus cv. Florida, (Jacquin\u007F\u007F : Fries) Kummer
				// Jacquin = Nikolaus Joseph von Jacquin (or Jacq.)
				// Hiern = William Philip Hiern

				// some \\r\\n weirdness... that's how it is in the gni database. with two \'s
				//22465622: Arrhenatherum tuberosum ssp. baeticum (Romero\\r\\nZarco) Rivas Mart. , Fern. Gonz. & Sánchez Mata
				//22441384: Lithospermum arvense ssp. sibthorpianum\\r\\nLithospermum arvense L. ssp. sibthorpianum (Griseb.) Stoj (Griseb.) Stoj. & Stef.
				//22427736: Trichiurus japonicus Temminck & Schlegel, 1844\\r\\nTemminck & Schlegel 1844

				string escaped = record.name.EscapeToCSharpLiteral();
				if (record.name != escaped.Replace(@"\n","\n").Replace(@"\""",@"""").Replace(@"\'", @"'").Replace(@"\\", @"\")) { // skip \n and ' and " and \ because too common
					Console.WriteLine(record.id + ": " + escaped);
					//Console.WriteLine(record.id + " as C# literal: " + escaped);
					//Console.WriteLine(record.id + " unescaped: " + escaped.UnescapeSimple());
					//Console.WriteLine(record.id + " fixed?: " + GNIStrings.RepairEncodingPlus(record.value));
				}
			}
		}

		public void ListBadUnicode() {
			int badrecords = 0;
			int total = 0;
			Dictionary<int, int> records = new Dictionary<int, int>(17300000); // 16887220 total.. meant to be 17,275,622 name strings total (website).. hrmm.. probably missing some

			// uses much memory. try running as "Release" if having errors.
			foreach (var record in Records()) {
				records[record.name.GetHashCode()] = record.int_id;
			}

			// dumb shit like "AntonÃn" wont be detected yet
			// todo: normalize curly quotes, en-/em-dashes
			// interesting:
			foreach (var record in Records()) {
				total++;
				string fixedName = GNIStrings.RepairEncodingMaybeMore(record.name);
				if (record.name != fixedName) {

					Console.WriteLine("Record: {0}", record.id);
					Console.WriteLine("http://gni.globalnames.org/name_strings/{0}", record.id);
					//Console.WriteLine("http://gni.globalnames.org/name_strings/{0}.xml", record.id);
					//Console.WriteLine("http://gni.globalnames.org/name_strings/{0}.json", record.id);
					Console.WriteLine("Existing name:  " + record.name);
					Console.WriteLine("Suggested name: " + fixedName);
					if (records.ContainsKey(fixedName.GetHashCode())) {
						Console.WriteLine(" = record: " + records[fixedName.GetHashCode()] + " "); // add url?
					} else {
						Console.WriteLine(" (did not find matching record)");
					}
					//Console.WriteLine("Suggested name: " + record.value.FixUTF(Encoding.GetEncoding("Windows-1252"))); // made default now
					Console.WriteLine();
					badrecords++;
				}
			}
			Console.WriteLine("Bad records: {0}", badrecords);
			Console.WriteLine("Total records: {0}", total);
		}


		public void ListMultilineRecords() {
			foreach (var record in Records()) {
				if (record.name.IndexOfAny(new char[] {'\n', '\r'}) != -1) {
					Console.WriteLine("Record: {0}", record.id);
					Console.WriteLine("http://gni.globalnames.org/name_strings/{0}", record.id);
					//Console.WriteLine("http://gni.globalnames.org/name_strings/{0}.xml", record.id);
					Console.WriteLine("http://gni.globalnames.org/name_strings/{0}.json", record.id);
					Console.WriteLine();
					Console.WriteLine(record.name);
					Console.WriteLine();
				}
			}
		}

		//suspicious: 3134 of 16887220 : 0.02% (weird camel)
		//repairable: 11822 of 16887220 : 0.07%
		//repairable: 14756 of 16887220 : 0.09% // updated list of characters
		//TODO separate out suspicious count (which is very slow)
		public void RepairablePercent() {
			int total = 0;
			int repairable = 0;
			int suspicious = 0;
			foreach (var record in Records()) {
				string repaired = GNIStrings.Repair(record.name);
				if (repaired != record.name) {
					repairable++;
				}
				if (GNIStrings.IsSuspiciousCamel(record.name, true)) {
					suspicious++;
				}
				total++;
			}

			Console.WriteLine("suspicious: {0} of {1} : {2:0.00}%", suspicious, total, (double)suspicious / total * 100);
			Console.WriteLine("repairable: {0} of {1} : {2:0.00}%", repairable, total, (double)repairable / total * 100);
		}

		// unknownPlacement: 6903 of 16887220 : 0.04%
		// unknownPlacement: 6910 of 16887220 : 0.04%
		// unknownPlacement: 6909 of 16887220 : 0.04%
		public void UnknownPlacementPercent() {
			int total = 0;
			int unknownPlacement = 0;
			foreach (var record in Records()) {
				string repaired = GNIStrings.Repair(record.name);
				if (Bioparser.IsUnknownPlacement(repaired)) {
					unknownPlacement++;
					Console.WriteLine(repaired);
				}
				total++;
			}

			Console.WriteLine("unknownPlacement: {0} of {1} : {2:0.00}%", unknownPlacement, total, (double)unknownPlacement / total * 100);
		}


		//17849981: Paridea costata CH&#x000DB;J&#x000D4;
		//18212272: Pseudachorutes lapponicus &#x000C5;gren
		//18212273: Pseudachorutes silvalicus &#x000C5;gren
		public void TestForChujoisms() {
			// are there others like this? 
			// name.Replace("Ch&#x000FB;j&#x000F4;", "Chûjô");

			foreach (var record in Records()) {
				string repaired = GNIStrings.RepairHTMLEncoding(record.name);
				//if (record.name.Contains("&#x")) {
				if (repaired != record.name) {
					//if (record.name.Contains("Ch&#x000FB;j&#x000F4;")) {
					//	Console.WriteLine("Chujo:");
					//}

					Console.WriteLine("{0}: {1}", record.id, record.name);
					Console.WriteLine("repaired: " + repaired);

					//Console.WriteLine();
				}
			}
		}

		virtual public void TestMaxId() {
			long max = long.MinValue;
			foreach (var item in Records()){ 
				int int_id = int.Parse( item.id );
				long long_id = long.Parse( item.id );
				if (int_id != long_id) {
					Console.WriteLine("no match: " + long_id + " != " + int_id);
				}
				if (long_id > max)
					max = long_id;
			}

			Console.WriteLine("max id: " + max);
			//max id:   22759397 (fits in an int; long not needed)
			//maxint: 2147483647
		}





	}
}

