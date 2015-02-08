using System;
using DotNetWikiBot;

namespace beastie {
	public class BeastieBot : PengoBot
	{

		private BeastieBot() : base("en.wikipedia.org", "003", true) {
		}

		static BeastieBot _instance;
		public static BeastieBot Instance() {
			if (_instance == null) {
				_instance = new BeastieBot();
			}

			return _instance;
		}

		public Page GetLoadedPage(string pageName, bool followRedirect) {
			var page = new Page(site, pageName);
			page.Load();
			//Console.WriteLine("redirect: " + site.regexes["redirect"] );
			//Console.Out.Flush();

			return page;
		}

		public XowaPage GetPage(string pageName, bool followRedirect) {
			var page = xowaDB.ReadXowaPage(pageName);
			if (followRedirect && page != null && !string.IsNullOrEmpty(page.text) && page.IsRedirect()) {
				var rto = page.RedirectsTo();
				page = xowaDB.ReadXowaPage(rto);
			}
			return page;
		}

		public string PageNameInWiki(string page) {
			var xowapage = xowaDB.ReadXowaPage(page);
			if (xowapage == null) {
				return null;
			}

			bool isRedir = xowapage.IsRedirect();
			if (isRedir) {
				string rto = xowapage.RedirectsTo();
				var rpage = xowaDB.ReadXowaPage(rto);
				if (rpage != null) {
					return rpage.title;
				}
			} 

			return xowapage.title; // may have underscores
		}


	}

}

