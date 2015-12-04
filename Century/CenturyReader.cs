using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;
using System.Runtime.Serialization;
using LumenWorks.Framework.IO.Csv;
using System.IO.Compression;

namespace beastie {
	public class CenturyReader
	{
		string testPath = @"C:\ngrams\datasets-gutenburg\Century Dictionary\daisy\centurydictionar03whit_daisy.zip";
		string testFileStem = @"centurydictionar03whit_daisy";
		string searchTermsFile = @"C:\ngrams\datasets-generated\obsolete-genus-list.txt";

		public List<string> searchTerms = new List<string>();

		public CenturyReader() {

		}

		public void Test() {
			LoadSearchTerms();
			SearchDaisy(testPath);
		}

		public void LoadSearchTerms() {
			var input = new StreamReader(searchTermsFile, Encoding.UTF8, true);
			using (input) {
				string line = null;
				while ((line = input.ReadLine()) != null) {
					if (line == null)
						return;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					if (line.Length <= 2)
						continue;

					searchTerms.Add(line.Trim());
				}
			}
		}

		public void SearchDaisy(string filename) {

			using (FileStream zipToOpen = new FileStream(filename, FileMode.Open))
			{

				using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
				{
					foreach (ZipArchiveEntry entry in archive.Entries) {
						if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) {
							//entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
							var reader = new StreamReader(entry.Open());
							string fileContents = reader.ReadToEnd();
							SearchWithinString2(fileContents);
						}
						
					}
				}
			}

		}

		// too slow
		public void SearchWithinString(string withinHere) {
			//TODO: care about word boundaries
			foreach (string term in searchTerms) {
				if (withinHere.IndexOf(term) != -1)
					Console.WriteLine(term);
			}
		}

		public void SearchWithinString2(string withinHere) {
			//TODO: care about word boundaries
			
			string pattern = "(" + searchTerms.Select(t => @"\b" + Regex.Escape(t) + @"\b").JoinStrings("|") + ")";
			//Regex regex = new Regex(pattern);
			Console.WriteLine(pattern);

			foreach (Match m in Regex.Matches(withinHere, pattern)) {
				//Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
				Console.WriteLine("{0}", m.Value);
			}
		}
	}
}

