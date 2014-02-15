using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;

//books (Cliff, Democracy guy): the advertised mind, seducing the subconscious

namespace beastie
{
	//TODO: create vocabprimer.com
	public class NgramReader
	{
		int startYear = 1950;

		public Dictionary<string, long> matchCounts = new Dictionary<string, long>(); // lemma -> match_count (since 1950)

		public NgramReader ()
		{
		}

		public void ReadIntoDatabase(string filename) {
			string prevLemma = null;
			int lineCount = 0;

			using (GZipStream stream = new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), CompressionMode.Decompress)) 
			using (StreamReader reader = new StreamReader(stream)) 
			//using (var conn = new SQLiteConnection(@"Data Source=ngramdatabase.db3"))
			//using (var cmd = conn.CreateCommand()) 
			{
				//conn.Open();

				while (true) {
					string line = null;
					try {
						line = reader.ReadLine();
					} catch (Exception e) {
						break;
					}
					if (line == null) break;
					lineCount++;

					//example line:
					// avgas_NOUN	1947	20	9
					// lemma TAB year TAB match_count TAB volume_count

					//TODO: only count "volume_count" of canonical form to avoid false duplicates

					string[] parts = line.Split(new char[]{'\t'});

					string rawLemma = parts[0];
					string lemma = CleanLemma(rawLemma);

					int year = 0;
					int.TryParse(parts[1], out year);

					long match_count;
					long.TryParse(parts[2], out match_count);

					long volume_count;
					long.TryParse(parts[3], out volume_count);

					if (year > startYear) {
						if (matchCounts.ContainsKey(lemma)) {
							matchCounts[lemma] += match_count;
						} else {
							matchCounts[lemma] = match_count;
						}
					}

					if (lemma != prevLemma) {
						//Console.WriteLine("{0} ({1})", lemma, year);
					}
					prevLemma = lemma;
				}
			}
			//Console.WriteLine("Line count: {0}", lineCount); // 86,618,505 for 'a' (googlebooks-eng-all-1gram-20120701-a)
		}

		public void PrintAll(string outputFilename) {


			using (StreamWriter writer = new StreamWriter( new FileStream(outputFilename, FileMode.Create, FileAccess.Write), System.Text.Encoding.UTF8)) {
				writer.NewLine = "\n";

				foreach (KeyValuePair<string,long> item in matchCounts.OrderByDescending(key=> key.Value)) { 
					writer.WriteLine("{0},{1}", item.Key, item.Value);
				}

			}
		}

		public static string CleanLemma(string lemma) {
			// remove trailing POS e.g. atavic_ADJ attaccato_DET
			// remove periods and anything after: afternoon.we anything.there
			// make lowercase`
			// todo: remove trailing numbers or symbols e.g. ate' atoms.1 attitude.8_NOUN αt_. avow_ account31

			// note: split joined words? architectengineering
			// note, allow?: astronomy.com
			// todo: slashes?: atoms/m3

			if (lemma.Length >= 2)  {
				if (lemma.StartsWith("_")) {
					lemma = lemma.Substring(1, lemma.Length-1);
				}
			}
			
			if (lemma.Length >= 2)  {
				int underscore = lemma.IndexOf('_',1);
				if (underscore != -1) lemma = lemma.Substring(0, underscore);

				int period = lemma.IndexOf('.',1);
				if (period != -1) lemma = lemma.Substring(0, period);
			}

			//while (! Char.IsLetter(lemma[lemma.Length-2])) {
			//	lemma = lemma.Substring(0, lemma.Length-2);
			//}

			lemma = lemma.ToLowerInvariant(); // CultureInfo.InvariantCulture

			if (lemma.EndsWith("'s")) {
				lemma = lemma.Substring(0, lemma.Length - "'s".Length);
			}
			    
			if (lemma.EndsWith("'ll")) {
				lemma = lemma.Substring(0, lemma.Length - "'ll".Length);
			}

			return lemma;
		}



	}
}

