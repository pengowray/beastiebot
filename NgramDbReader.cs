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
	public class NgramDbReader
	{

		string corpus = "unknown";
		int startYear = 1950;

		public NgramDbReader (string corpus, int startYear = 1950) {
			this.corpus = corpus;
			this.startYear = startYear;
		}

		public void ReadFile(string filename) {

			Console.WriteLine("Reading ngrams from {0}", filename);

			int lineCount = 0;

			using (GZipStream stream = new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), CompressionMode.Decompress)) 
			using (StreamReader reader = new StreamReader(stream)) 
			using (NgramDatabase ngramDatabase = new NgramDatabase())
			{
				ngramDatabase.CreateTables();

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

					Lemma lemma = new Lemma(rawLemma);
					//CleanLemma(rawLemma);
					//string stem = Stem(Lower(rawLemma));

					int year = 0;
					int.TryParse(parts[1], out year);

					long match_count;
					long.TryParse(parts[2], out match_count);

					long volume_count;
					long.TryParse(parts[3], out volume_count);

					if (year < startYear) continue;

					long matchAdd = 0;
					long volumeAdd = 0;

					if (! lemma.hasPos) matchAdd = match_count;
					if (lemma.isCanonicalUnchangedCase) volumeAdd = volume_count;

					if (volumeAdd > 0 || matchAdd > 0) {
						ngramDatabase.AddLemmaCounts(lemma.cleaned, corpus, matchAdd, volumeAdd);
					}
				}
			
				//Console.WriteLine("Line count: {0}", lineCount); // 86,618,505 for 'a' (googlebooks-eng-all-1gram-20120701-a)
			}
		}
		


	}
}

