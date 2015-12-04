using System;
using System.Linq;
using DotNetWikiBot;

namespace beastie {
	public class FixCatEponyms
	{
		public FixCatEponyms() {
		}

		// find entries missing [[Category:Translingual taxonomic eponyms]]
		public void PrintList() {
			Console.WriteLine("ok");

			// try with both %ae and %i
			// also %ia and %la for genera
			// %% for all
			var ae = WiktionaryBot.Instance().xowaDB.PagesLike("%ae");

			bool quick = false; // if true, just use xowa and don't check the real site

			foreach (var page in ae) {
				//check if translingual
				var wiktEntry = page.ToWiktionaryEntry();
				if (!wiktEntry.Sections().ContainsKey("Translingual"))
					continue;

				//check if category already there
				var cats = page.ToPage().GetCategories();
				if (cats.Select(c => c.Replace("_", " ")).Any(c => c.Contains("Translingual taxonomic eponyms")))
					continue;
					
				string mul = wiktEntry.Sections()["Translingual"];

				if (mul.Contains("having English names") || 
					mul.Contains("pseudo-Latin") ||
					mul.Contains("'s") ||
					mul.Contains("named for") ||
					mul.Contains("named after") ||
					mul.Contains("after") ||
					mul.Contains("honor") ||
					mul.Contains("honour") ||
					mul.Contains("Latinized") ||
					mul.Contains("Latinised") ||
					mul.Contains("first name") ||
					mul.Contains("last name") ||
					mul.Contains("surname") ||
					mul.Contains("genitive") ||
					mul.Contains("eponym")) {

					// replace above "if" with this one to just get all lowercase titles
				//if (page.title[0].ToString() == page.title[0].ToString().ToLowerInvariant()) {

					if (!quick) {
						Bot.EnableSilenceMode();
						//check if category already there for real this time
						var realPage = WiktionaryBot.Instance().RetrievePage(page.title);
						if (realPage.Exists()) {
							var realCats = realPage.GetCategories();
							if (realCats.Select(c => c.Replace("_", " ")).Any(c => c.Contains("Translingual taxonomic eponyms")))
								continue;
						}
						Bot.DisableSilenceMode();
					}

					Console.WriteLine("*[[" + page.title + "]]");

				}
				//Console.WriteLine(page.text);
			}
		}
	}
}

