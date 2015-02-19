using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class PercentDone : NgramReader {
		//private Dictionary<string, long> epithetScore = new Dictionary<string, long> ();

		public bool quick = true;
		public int startYear = 0;

		private Dictionary<string, long> matchCount = new Dictionary<string, long> ();
		private Dictionary<string, long> todoCount = new Dictionary<string, long> ();

		string what = "something";
			
		WiktionaryBot wikt;

		public PercentDone() {
			wikt = WiktionaryBot.Instance();
		}

		protected override void ProcessLine(string line) {
			//example line:
			//Dicerorhinus sumatrensis	2005	69	34
			//lemma TAB year TAB match_count TAB volume_count

			Ngram ngram = new Ngram(line);

			if (ngram.year <= startYear) {
				return;
			}

			string speciesString = ngram.lemma.raw;

			var sp = new Species(speciesString);

			string name = sp.epithet; what = "epithets";
			//string name = sp.genus; what = "genera";
			//string name = sp.ToString(); what = "binomial names";

			if (sp.epithet == "sp" || sp.epithet == "sp.") // ignore junk
				return;

			if (matchCount.ContainsKey(name)) {
				matchCount[name] += ngram.match_count;
			} else {
				matchCount[name] = ngram.match_count;
			}

		}

		public void PrintResults() {
			long totalBookMentions = matchCount.Values.Sum();
			long completeBookMentions = 0;

			long totalCount = matchCount.Values.Count();
			long completeCount = 0;


			foreach (var entry in matchCount) {
				bool epExists = wikt.ExistsMulLa(entry.Key, quick);
				if (epExists) {
					completeBookMentions += entry.Value;
					completeCount++;
				} else {
					todoCount[entry.Key] = entry.Value;
				}
			}

			Console.WriteLine("since year: " + startYear);
			PrintResult("Complete of all " + what + " mentions in English-language books", completeBookMentions, totalBookMentions, true);
			PrintResult("Complete " + what + " (the ones that are seen at least a few times in books)", completeCount, totalCount);
		}

		void PrintResult(string name, long complete, long total, bool showNeededForNext = false) {
			double percentDone = (double)complete / (double)total * 100;
			Console.WriteLine("{0}: {1:0.00}%  ({2} / {3})", 
				name,
				percentDone,
				complete,
				total);

			if (showNeededForNext) {
				//NeededForNext(complete, total, .25, true);
				//NeededForNext(complete, total, .30, true);
				//NeededForNext(complete, total, .50);
				NeededForNext(complete, total, .75, true);
				NeededForNext(complete, total, .80, true);
				NeededForNext(complete, total, .85);
				NeededForNext(complete, total, .90);
				NeededForNext(complete, total, .95);
				NeededForNext(complete, total, .99);
				NeededForNext(complete, total, 1.0);
			}

		}

		void NeededForNext(long currentComplete, long total, double wanted, bool nameThem = false) {
			long needed = (long) (total * wanted);
			var ordered = todoCount.OrderByDescending(k => k.Value);



			float neededComplete = currentComplete;
			long items = 0;
			foreach (var kvp in ordered) {
				neededComplete += kvp.Value;
				items++;

				if (neededComplete > needed) {
					Console.WriteLine("* to reach {0:0}%: {1}", wanted * 100, items);

					if (nameThem) {
						string namingThem = ordered.Take((int)items).Select(o => "[[" + o.Key + "#Latin|" + o.Key + "]]").JoinStrings(", ");
						Console.WriteLine("{0}", namingThem);
					}

					return;
				}
			}


			Console.WriteLine("Can never make it. :(");
		}

	}
}

