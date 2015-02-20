using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//never used
namespace beastie {
	public class PreviouslyPopular  : NgramReader
	{
		//WiktionaryBot wikt = null;

		int startYear = 1911;

		public PreviouslyPopular() {

		}

		Dictionary<string, long> counts = new Dictionary<string, long>();

		protected override void ProcessLine(string line) {
			//example line:
			//Dicerorhinus sumatrensis	2005	69	34
			//lemma TAB year TAB match_count TAB volume_count

			Ngram ngram = new Ngram(line);

			string species = ngram.lemma.raw;
			int weight = (ngram.year <= startYear ? 1 : -1);
			//Species sp = new Species(speciesName);

			if (counts.ContainsKey(species)) {
				counts[species] += weight;
			} else {
				counts[species] = weight;
			}
		}

		public void PrintResults() {
			Console.WriteLine("Most popular before " + startYear + " (and not after)");
			var top = counts.Where(c => c.Value > 0).OrderByDescending(c => c.Value).ThenBy(c => c.Key.ToString()).Take(1000);
			foreach (var sp in top) {
				Console.WriteLine("# " + sp.Key);
			}
		}

	}
}