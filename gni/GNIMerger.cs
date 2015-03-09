using System;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//using CSharpParser.Utility;
using beastie;

namespace beastie {


	// merging is done. use GNICsv instead.


	public class GNIMerger : GNICsv
	{
		// gni-download.csv-aaa-car.txt  "gni-download.csv-caa-eri.txt",
		string[] files = new string[] { "gni-download.csv-aaa-car.txt", "gni-download.csv-caa-dzz.txt",
			"gni-download.csv-eaa-per.txt", "gni-download.csv-paa-zzz.txt"
		};

		string path = @"D:\ngrams\datasets-gni\";
		//string path = @"D:\ngrams\datasets-gni\delete me--all-merged\";

		public GNIMerger() {
		}



		IEnumerable<GniItem> Records(bool showFilenames = false) {
			//TODO
			return null;
		}

		public override IEnumerable<GniItem> Records() {
			HashSet<int> seenIds = new HashSet<int>();
			//foreach (var item in RecordsWithDuplicates(showFilenames)) {
			foreach (var item in RecordsWithDuplicates()) {
				int id = int.Parse(item.id);
				if (seenIds.Contains(id))
					continue;

				seenIds.Add(id);
				yield return item;
			}
		}

		override public void TestMaxId() {
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
			// database:       `id` int(11) UNSIGNED NOT NULL AUTO_INCREMENT, // will always fit in a 32-bit int.

		}

		public IEnumerable<GniItem> RecordsWithDuplicates(bool showFilenames = false) {
			//long lineCount = 1;
			foreach (string file in files) {
				if (showFilenames) 
					Console.WriteLine("****** " + file);
				using (var infile = new StreamReader(path + file, Encoding.UTF8, true)) { // Windows-1252 encoding, not UTF-8. (e.g. for "Galápagos")
					CsvReader csv = new CsvReader(infile, true);
					string[] headers = csv.GetFieldHeaders();
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