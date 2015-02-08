using System;
using DotNetWikiBot;

namespace beastie {
	public class PengoBot
	{
		public string domain;
		string text_table_file_index;
		bool uppercaseFirstLetter;

		private Site _site;
		public Site site {
			get {
				if (_site == null) {
					//DotNetWikiBot
					//Site site = new Site("https://en.wikipedia.org", "YourBotLogin", "YourBotPassword");
					Bot.cacheDir = @"C:\Cache"; //TODO: move this somewhere and/or make configurable
					_site = new Site("https://" + domain); // login details required in /Cache/Defaults.dat

					//Old method, using LinqToWiki (was broken somehow. Try again if really need more advanced queries)
					//Docs: https://en.wikipedia.org/wiki/User:Svick/LinqToWiki
					//var wiki = new Wiki("BeastieBot/1.0 (http://en.wikipedia.org/wiki/User:Beastie_Bot (running read-only))", "en.wikipedia.org", "/w/api.php");
				}
				return _site;
			}
		}


		private XowaDB _xowaDB;
		public XowaDB xowaDB {
			get {
				if (_xowaDB == null) {
					_xowaDB = new XowaDB(domain);
					_xowaDB.uppercaseFirstLetter = uppercaseFirstLetter;
					_xowaDB.text_table_file_index = text_table_file_index;
				}
				return _xowaDB;
			}
		}


		public PengoBot(string domain, string text_table_file_index, bool upperFirstLetter) {
			this.domain = domain;
			this.uppercaseFirstLetter = upperFirstLetter;
			this.text_table_file_index = text_table_file_index;
		}


	}
}

