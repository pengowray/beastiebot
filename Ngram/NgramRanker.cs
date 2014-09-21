using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace beastie
{
	public class NgramRanker
	{
		// ranked dictionary of all words //TODO: replace with database
		Dictionary<string, int> wordRanks = new Dictionary<string, int>(); // word -> ranking (#1 is most frequent)
		Dictionary<string, string> actual = new Dictionary<string, string>(); // stem or normalized -> the last actual representation in the text (capitalized, non-stemmed)

		// from text:
		//TODO: keep track of counts, context, etc. and put in a database
		Dictionary<string, int> wordsInText = new Dictionary<string, int>(); // word -> ranking (#1 is most frequent)
		HashSet<string> notRanked = new HashSet<string>();

		Dictionary<string, string[]> languages = new Dictionary<string, string[]>(); 
		HashSet<string> foreignWords = new HashSet<string>(); 

		bool isStems = false;

		public NgramRanker ()
		{
		}

		public void SetMassagedDataStems(string filename) {
			SetMassagedData(filename, true);
		}
		public void SetMassagedData(string filename, bool isStems = false) {
			this.isStems = isStems;
			using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) 
			using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8)) {
				int lineCount = 0;
				while (true) {
					string line = null;
					try {
						line = reader.ReadLine();
					} catch (Exception e) {
						break;
					}
					if (line == null) break;
					lineCount++;

					string[] parts = line.Split(new char[]{','});
					if (parts.Length != 2) continue;
					string lemma = parts[0];

					wordRanks[lemma] = lineCount;
					//Console.WriteLine("Massaged data: {0} -> {1}", lemma, lineCount);
				}

				//Console.WriteLine("Massaged data read: {0}", lineCount);
			}
		}

		public void RankText(string filename) {
			WiktionaryData wiktionaryData = WiktionaryData.Instance();

			using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) 
			using (StreamReader reader = new StreamReader(stream)) {
				//TODO: ignore Guttenburg boilerplate text
				//TODO: understand page numbers and chapters

				int lineCount = 0;
				while (true) {
					string line = null;
					try {
						line = reader.ReadLine();
					} catch (Exception e) {
						break;
					}
					if (line == null) break;
					if (line.StartsWith("***END OF THE PROJECT GUTENBERG EBOOK")) break;
					lineCount++;

					//Regex wordsRegex = new Regex(@"\b\w+\b");
					//foreach (Match match in wordsRegex.Matches(line)) {
					// match.Value, match.Index;

					Regex r = new Regex(@"(\b[\w\']+\b)", RegexOptions.IgnoreCase);
					Match m = r.Match(line);

					while (m.Success) {
						CaptureCollection cc = m.Groups[0].Captures;
						for (int j = 0; j < cc.Count; j++) {
							Capture c = cc[j];
							//c.Index;
							//c
							string rawWord = c.ToString(); 

							string word = NgramStemCounter.CleanLemma(rawWord);

							// language via wiktionary
							string[] langs = WiktionaryDatabase.LanguagesOfTerm(word);
							if (!langs.Contains("en") && langs.Length >= 2) foreignWords.Add(word);
							languages[word] = langs;

							string normalized = NgramStemCounter.Lower(word);
							if (isStems) {
								normalized = NgramStemCounter.Stem(normalized);
							}

							actual[normalized] = word;

							if (wordRanks.ContainsKey(normalized)) {
								wordsInText[normalized] = wordRanks[normalized];
							} else {
								notRanked.Add(normalized);
							}
						}
						m = m.NextMatch();
					}
				}
			}

		}

		public void PrintTop() {
			foreach (string item in notRanked.OrderBy(value=>value)) { 
				Console.WriteLine("{0},-1,{1}", item, actual[item]);
			}
			foreach (KeyValuePair<string,int> item in wordsInText.OrderByDescending(key=>key.Value)) { 
				Console.WriteLine("{0},{1},{2}", item.Key, item.Value, actual[item.Key]);
			}

			WiktionaryData wiktionaryData = WiktionaryData.Instance();
			foreach (string word in foreignWords) {
				try {
					string langs = WiktionaryDatabase.LanguagesOfTerm(word).Select(w => wiktionaryData.codeIndex[w].canonicalName).JoinStrings(", ");
					Console.WriteLine("{0}: {1} ({2})", word, langs, languages[word].JoinStrings(", "));
				} catch (System.Collections.Generic.KeyNotFoundException e) {
					Console.WriteLine("{0}: key not found.", word);
					continue;
				}
			}

		}
	}
}

