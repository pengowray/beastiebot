using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class NgramSpeciesTally : NgramReader {

		private Dictionary<string, long> volumeCount = new Dictionary<string, long> ();

		//public int startYear = 0; // all
		public int startYear = 1950; // 1950;

		public NgramSpeciesTally() {
		}

		protected override void ProcessLine(string line) {
			//example line:
			//Dicerorhinus sumatrensis	2005	69	34
			//lemma TAB year TAB match_count TAB volume_count

			Ngram ngram = new Ngram(line);

			if (ngram.year <= startYear) {
				return;
			}

			string species = ngram.lemma.raw;

			if (volumeCount.ContainsKey(species)) {
				volumeCount[species] += ngram.volume_count;
			} else {
				volumeCount[species] = ngram.volume_count;
			}
		}

		public void OutputToFile(string filename) {
			var sorted = from entry in volumeCount orderby entry.Value descending select entry;

			if (filename == null || filename == "") {
				foreach (var sp in sorted) {
					Console.WriteLine("{0},{1}", sp.Key, sp.Value);
				}
			} else {
				var output = new StreamWriter(filename, false, Encoding.UTF8);
				foreach (var sp in sorted) {
					output.WriteLine("{0},{1}", sp.Key, sp.Value);
				}
				output.Close();
			}

			Console.WriteLine("Species: {0}", volumeCount.Count); 
		}

	}
}
