using System;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//using CSharpParser.Utility;
using beastie;

namespace beastie {
	public class GNIMerger
	{
		// gni-download.csv-aaa-car.txt  "gni-download.csv-caa-eri.txt",
		string[] files = new string[] { "gni-download.csv-aaa-car.txt", "gni-download.csv-caa-dzz.txt",
			"gni-download.csv-eaa-per.txt", "gni-download.csv-paa-zzz.txt"
		};
		string path = @"D:\ngrams\datasets-gni\";
		string[] headers;

		public GNIMerger() {
		}

		public void OutputMergedCsv() {
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
				if (GNIStrings.IsCharacterChoiceSuspicious(name, true)) {
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
				string fixedName = GNIStrings.RepairEncodingPlus(record.name);
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

		public void TestMaxId() {
			long max = long.MinValue;
			foreach (var item in RecordsWithDuplicates(true)){ 
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

		IEnumerable<GniItem> Records(bool showFilenames = false) {
			HashSet<int> seenIds = new HashSet<int>();
			foreach (var item in RecordsWithDuplicates(showFilenames)) {
				int id = int.Parse(item.id);
				if (seenIds.Contains(id))
					continue;

				seenIds.Add(id);
				yield return item;
			}
		}

		IEnumerable<GniItem> RecordsWithDuplicates(bool showFilenames) {
			//long lineCount = 1;
			foreach (string file in files) {
				if (showFilenames) 
					Console.WriteLine("****** " + file);
				using (var infile = new StreamReader(path + file, Encoding.UTF8, true)) { // Windows-1252 encoding, not UTF-8. (e.g. for "Galápagos")
					CsvReader csv = new CsvReader(infile, true);
					headers = csv.GetFieldHeaders();
					csv.SupportsMultiline = true;

					while (csv.ReadNextRecord()) {
						var item = new GniItem();
						item.id = csv[0];
						item.name = csv[1]; // .CsvUnescape();
						yield return item;
					}

					/*
					while ((line = infile.ReadLine()) != null) {

					infile.ReadLine(); // headers
					lineCount++;
					string line;
					while ((line = infile.ReadLine()) != null) {
						lineCount++;
						var csv = line.Split(new char[] {','}, 2);
						var item = new GniItem();
						if (csv.Length != 2) {
							Console.WriteLine("MalformedCsvException");
							Console.WriteLine("line count: " + lineCount);
							Console.WriteLine("last line: " + line);
						} else {
							if (!csv[1].EndsWith("\"")) {
								csv[1] += " " + infile.ReadLine();
								lineCount++;
								Console.WriteLine(lineCount + " two liner: " + csv[1]);
							}
							item.id = csv[0];
							item.value = csv[1].CsvUnescape();
							yield return item;
						}
					}
					*/
					/*
					if (errorflag) { 
						Console.WriteLine("MalformedCsvException");
						Console.WriteLine("last line: " + csv[0] + "," + csv[1]);
						errorflag = false;
						continue; // skip to next file
					}
					*/
				}
			}
		}

	}
}

public class GniItem {
	public string id;
	public string name;
	public int int_id {
		get {
			return int.Parse(id);
		}
	}


}


//Existing name:  Geastrum corollinum (Batsch) HollÃ›s
//Suggested name: Geastrum corollinum (Batsch) HollÛs

//Existing name:  Dermonema virens (J. Agardh) Pedroche & Ã*vila OrtÃ­z
//Suggested name: Dermonema virens (J. Agardh) Pedroche & �*vila Ortíz
//string good = "Ávila Ortíz";
//string bad =  "Ã*vila OrtÃ­z";
//string bad =  "Ãvila OrtÃ­z";



/*
// no fix found
//Existing name:  Adelophis DugÃ‹s, 1879
//Suggested name: Adelophis DugËs, 1879
string good = "Dugès";
string bad =  "DugÃ‹s";
bad.FindEncoding(good, true);
Console.WriteLine( bad.FixUTF()); 
Console.WriteLine( bad.FixUTFv2());

/*
// no fix found
//Existing name:  Achimenes hintoniana RamÃ†rez Roa & L.E. Skog
//Suggested name: Achimenes hintoniana RamÆrez Roa & L.E. Skog
//Should be:      Achimenes hintoniana Ramírez Roa & L.E. Skog 
string good = "Ramírez";
string bad =  "RamÃ†rez";
bad.FindEncoding(good);
Console.WriteLine( bad.FixUTF()); 
Console.WriteLine( bad.FixUTFv2());
*/
/*
//just munted. no fix.
//Existing name:  Acalypha neomexicana MÆ’ll. Arg.
//Suggested name: Acalypha neomexicana Mƒll. Arg.
string good = "Müll";
string bad =  "MÆ’ll";
bad.FindEncoding(good);
Console.WriteLine( bad.FixUTF()); 
Console.WriteLine( bad.FixUTFv2());
*/

/*
// bad fix
//Existing name:  Ceratophysella bengtssoni (Ågren, H, 1904) Stach, J, 194
//Suggested name: Ceratophysella bengtssoni (�gren, H, 1904) Stach, J, 194
string bad =  "Ågren";
string good = "Ågren";
//            "A. v�lvarez" // fixed with Windows-1252
bad.FindEncoding(good);
Console.WriteLine( bad.FixUTF()); // messes it up
Console.WriteLine( bad.FixUTFv2()); // ok
*/


/*
// macintosh -- 10000 -- Western European (Mac)   [[Mac OS Roman]]
string bad =  "A.√Ålvarez";
string good = "A.Álvarez";
//            "A.v�lvarez" // fixed with Windows-1252
bad.FindEncoding(good);
Console.WriteLine( bad.FixUTFv2());

// Windows-1252
string bad2 = "Ã˜strup";
string good2 = "Østrup";
bad2.FindEncoding(good2);
Console.WriteLine( bad2.FixUTFv2());
*/