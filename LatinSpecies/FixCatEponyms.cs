using System;
using System.Linq;

namespace beastie {
	public class FixCatEponyms
	{
		public FixCatEponyms() {
		}

		// find entries missing [[Category:Translingual taxonomic eponyms]]
		public void PrintList() {
			Console.WriteLine("ok");
			var ae = WiktionaryBot.Instance().xowaDB.PagesLike("%i"); // try with both %ae and %i
			foreach (var page in ae) {
				//check if translingual
				var wiktEntry = page.ToWiktionaryEntry();
				if (!wiktEntry.Sections().ContainsKey("Translingual"))
					continue;

				//check if category already there
				var cats = page.ToPage().GetCategories();
				if (cats.Select(c => c.Replace("_", " ")).Contains("Translingual taxonomic eponyms"))
					continue;
					
				string mul = wiktEntry.Sections()["Translingual"];

				if (mul.Contains("having English names") || 
					mul.Contains("pseudo-Latin") ||
					mul.Contains("named for") ||
					mul.Contains("named after") ||
					mul.Contains("after") ||
					mul.Contains("honor") ||
					mul.Contains("honour") ||
					mul.Contains("Latinized") ||
					mul.Contains("Latinized") ||
					mul.Contains("first name") ||
					mul.Contains("last name") ||
					mul.Contains("surname") ||
					mul.Contains("genitive form") ||
					mul.Contains("eponym")) {

						Console.WriteLine("*[[" + page.title + "]]");
				}
				//Console.WriteLine(page.text);
			}
		}
	}
}

