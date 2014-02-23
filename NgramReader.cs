using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//books (Cliff, Democracy guy): the advertised mind, seducing the subconscious

namespace beastie
{
	//TODO: create vocabprimer.com
	public class NgramReader
	{
		static SF.Snowball.Ext.EnglishStemmer eng = new SF.Snowball.Ext.EnglishStemmer();
		
		int startYear = 1950;

		public Dictionary<string, long> stemMatchCounts = new Dictionary<string, long>(); // stem -> match_count (since 1950)
		public Dictionary<string, long> lemmaMatchCounts = new Dictionary<string, long>(); // lemma -> match_count (since 1950)
		// TODO: public Dictionary<string, long> topLemma = new Dictionary<string, string>(); // lemma -> stem

		public NgramReader ()
		{
		}

		public void ReadFile(string filename) {
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
					string stem = Stem(Lower(rawLemma));

					int year = 0;
					int.TryParse(parts[1], out year);

					long match_count;
					long.TryParse(parts[2], out match_count);

					long volume_count;
					long.TryParse(parts[3], out volume_count);

					if (year > startYear) {
						if (stemMatchCounts.ContainsKey(stem)) {
							stemMatchCounts[stem] += match_count;
						} else {
							stemMatchCounts[stem] = match_count;
						}

						if (lemmaMatchCounts.ContainsKey(lemma)) {
							lemmaMatchCounts[lemma] += match_count;
						} else {
							lemmaMatchCounts[lemma] = match_count;
						}

						//TODO: connect lemmas to stems

					}
				}
			}
			//Console.WriteLine("Line count: {0}", lineCount); // 86,618,505 for 'a' (googlebooks-eng-all-1gram-20120701-a)
		}

		public void PrintAllLemmas(string outputFilename) {
			using (StreamWriter writer = new StreamWriter( new FileStream(outputFilename, FileMode.Create, FileAccess.Write), System.Text.Encoding.UTF8)) {
				writer.NewLine = "\n";

				foreach (KeyValuePair<string,long> item in lemmaMatchCounts.OrderByDescending(key=> key.Value)) { 
					writer.WriteLine("{0},{1}", item.Key, item.Value);
				}

			}
		}

		public void PrintAllStems(string outputFilename) {
			using (StreamWriter writer = new StreamWriter( new FileStream(outputFilename, FileMode.Create, FileAccess.Write), System.Text.Encoding.UTF8)) {
				writer.NewLine = "\n";
				
				foreach (KeyValuePair<string,long> item in stemMatchCounts.OrderByDescending(key=> key.Value)) { 
					writer.WriteLine("{0},{1}", item.Key, item.Value);
				}
				
			}
		}

		public static string CleanLemma(string lemma) {
			// remove trailing POS e.g. atavic_ADJ attaccato_DET
			// remove periods and anything after: afternoon.we anything.there
			// doesn't make lowercase -- use Lower()
			// doesn't stem -- use Stem()

			// todo: remove trailing numbers or symbols e.g. ate' atoms.1 attitude.8_NOUN αt_. avow_ account31

			// note, allow?: astronomy.com
			// todo: slashes?: atoms/m3
			// note: split joined words? architectengineering

			if (lemma.Length >= 2)  {
				if (lemma.StartsWith("_")) {
					lemma = lemma.Substring(1, lemma.Length-1);
				}
			}
			
			if (lemma.Length >= 2)  {
				int underscore = lemma.IndexOf('_',1);
				if (underscore != -1) lemma = lemma.Substring(0, underscore);
			}

			if (lemma.Length >= 2)  {
				int period = lemma.IndexOf('.',1);
				if (period != -1) lemma = lemma.Substring(0, period);
			}

			lemma = Regex.Replace(lemma, @"[-.,\d]*$", ""); // remove trailing numbers, commas, periods

			//while (! Char.IsLetter(lemma[lemma.Length-2])) {
			//	lemma = lemma.Substring(0, lemma.Length-2);
			//}

			string lower = Lower(lemma);

			if (lower.EndsWith("'s")) {
				lemma = lemma.Substring(0, lemma.Length - "'s".Length);
			}
			    
			if (lower.EndsWith("'ll")) {
				lemma = lemma.Substring(0, lemma.Length - "'ll".Length);
			}

			return lemma;
		}

		public static string Lower(string word) {
			return word.ToLowerInvariant(); // CultureInfo.InvariantCulture
		}

		public static string Stem(string word) {
			//unattainability -> unattain
			//uglifying -> uglifi

			eng.SetCurrent(word);
			eng.Stem();
			return eng.GetCurrent();
		}

	}
}

