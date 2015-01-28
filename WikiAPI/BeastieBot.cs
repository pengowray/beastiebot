using System;
using DotNetWikiBot;

namespace beastie {
	public class BeastieBot
	{
		public readonly Site site;

		private BeastieBot() {
			//DotNetWikiBot
			//Site site = new Site("https://en.wikipedia.org", "YourBotLogin", "YourBotPassword");
			Bot.cacheDir = @"C:\Cache"; //TODO: move this somewhere and/or make configurable
			site = new Site("https://en.wikipedia.org"); // login details required in /Cache/Defaults.dat

			//Old method, using LinqToWiki (was broken somehow. Try again if really need more advanced queries)
			//Docs: https://en.wikipedia.org/wiki/User:Svick/LinqToWiki
			//var wiki = new Wiki("BeastieBot/1.0 (http://en.wikipedia.org/wiki/User:Beastie_Bot (running read-only))", "en.wikipedia.org", "/w/api.php");
		}

		static BeastieBot _instance;
		public static BeastieBot Instance() {
			if (_instance == null) {
				_instance = new BeastieBot();
			}

			return _instance;
		}


	}

}

