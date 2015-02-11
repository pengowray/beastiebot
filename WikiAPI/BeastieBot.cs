using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		public static TaxonDetails Taxobox(Page page) {

			bool taxobox_found = page.GetTemplates(false, false).Select(t => t.Trim()).Contains("Taxobox");

			if (!taxobox_found) {
				Console.WriteLine("Taxobox not found: " + page.title);
				Console.WriteLine("Templates: " + page.GetTemplates(false, false).JoinStrings(", "));
				return null;
			}

			//string taxoboxText = page.GetTemplates(true, false);

			//var taxobox = page.site.ParseTemplate(taxoboxText);

			//string name = taxobox["name"];
			//string regnum = taxobox["regnum"];
			//string phylum = taxobox["phylum"];

			string name   = page.GetFirstTemplateParameter("Taxobox", "name");
			string regnum = page.GetFirstTemplateParameter("Taxobox", "regnum");
			string phylum = page.GetFirstTemplateParameter("Taxobox", "phylum");

			Console.WriteLine(string.Format("title:{0}, etc: {1} {2} {3}", page.title, name, regnum, phylum));

			return null;
		}

		public static void TestTaxon(string taxon) {
			var xowaPage = BeastieBot.Instance().GetPage(taxon, true);
			if (xowaPage != null) {
				var page = xowaPage.ToPage();
				//var bluewhale = BeastieBot.Instance().GetLoadedPage("Blue whale", true);
				BeastieBot.Taxobox(page);
			}
		}

	}

}

