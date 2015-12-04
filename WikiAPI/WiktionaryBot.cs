using System;
using DotNetWikiBot;

//almost identical to BeastieBot
//TODO: combine with BeastieBot

namespace beastie {
	public class WiktionaryBot : PengoBot
	{

		private WiktionaryBot() : base("en.wiktionary.org", "002", false) {
		}

		static WiktionaryBot _instance;
		public static WiktionaryBot Instance() {
			if (_instance == null) {
				_instance = new WiktionaryBot();
			}

			return _instance;
		}

		public Page RetrievePage(string title) {
			Page page = new Page(site, title);
			//p.LoadWithMetadata();
			page.Load();
			return page;
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
			var entry = xowaDB.ReadWiktionaryEntry(title);
			if (entry != null && !string.IsNullOrEmpty(entry.text)) {
				if (ExistsMulLa(entry)) {
					return true;
				}
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

