using System;
using DotNetWikiBot;

//almost identical to BeastieBot
//TODO: combine with BeastieBot

namespace beastie {
	public class WiktionaryBot
	{
		public readonly Site site;
		XowaDB xowaDB;

		private WiktionaryBot() {
			//DotNetWikiBot
			//Site site = new Site("https://en.wikipedia.org", "YourBotLogin", "YourBotPassword");
			Bot.cacheDir = @"C:\Cache"; //TODO: move this somewhere and/or make configurable
			site = new Site("https://en.wiktionary.org"); // login details required in /Cache/Defaults.dat

			//Old method, using LinqToWiki (was broken somehow. Try again if really need more advanced queries)
			//Docs: https://en.wikipedia.org/wiki/User:Svick/LinqToWiki
			//var wiki = new Wiki("BeastieBot/1.0 (http://en.wikipedia.org/wiki/User:Beastie_Bot (running read-only))", "en.wikipedia.org", "/w/api.php");
		}

		static WiktionaryBot _instance;
		public static WiktionaryBot Instance() {
			if (_instance == null) {
				_instance = new WiktionaryBot();
			}

			return _instance;
		}

		public WiktionaryEntry RetrieveEntry(string title) {
			Page page = new Page(site, title);
			//p.LoadWithMetadata();
			page.Load();
			if (page.Exists()) {
				if (page.GetNamespace() != 0) {
					// something weird has happened.
					//TODO: throw something
					return null;
				}
				//Console.WriteLine(page.title);
				//Console.WriteLine(speciesPage.text);
				if (page.IsRedirect()) {
					string redirTo = page.RedirectsTo();
					Console.WriteLine(title + " => " + redirTo);
					//WikipediaPageName = redirTo;

					//speciesPage.IsDisambig(); // TODO
					//var cats = p.GetAllCategories();
				} else {
					var entry = new WiktionaryEntry();
					entry.title = page.title;
					entry.text = page.text;
					return entry;
				}
			}

			//TODO throw something?
			return null;
		}

		/**
		 * check if a wiktionary article exists in Latin or Translingual.
		 */
		public bool ExistsMulLa(string title, bool quick = false) {
			// first try XowaDB
			if (xowaDB == null) {
				xowaDB = new XowaDB();
			}

			var entry = xowaDB.ReadEntry(title);
			if (entry != null && !string.IsNullOrEmpty(entry.text)) {
				return ExistsMulLa(entry);
			}

			//TODO: or instead check pengo database (built from Wiktionary db dump)

			//TODO: cache results: certain groups of words, e.g. genus, epithet, species, and load that into a HashSet to check first. 
			//TODO: also include date last checked, and don't recheck for a month if found, (maybe 2 to 12 hours if not found? or always recheck?)

			if (quick) {
				return false; // don't bother checking online if doing a quick search
			}

			// then check the actual website
			var wikt = WiktionaryBot.Instance();
			entry = wikt.RetrieveEntry(title);
			if (entry != null) {
				return ExistsMulLa(entry);
			}

			Console.Error.WriteLine("Wiktionary mul/la Search failed: " + title);
			return false;

		}

		public bool ExistsMulLa(WiktionaryEntry entry) {
			var sections = entry.Sections();

			return sections.ContainsKey("Latin") || sections.ContainsKey("Translingual");
		}

	}



}

