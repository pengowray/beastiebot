using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetWikiBot;

namespace beastie {
	public class BeastieBot : PengoBot
	{

		// if it ends with one of these, it's probably not a common name (but need an exception list?)
		private string[] taxonEndings = new string[] { "idae", "aceae", "inae", "iformes", "oidei", "ini" };

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

		public string CommonNameFromWiki(Bitri bitri) {
			// attempts to guess the get the common name from a taxa name
			// returns null if not found, or is dubious

			string basicName = bitri.BasicName();
			var xowapage = xowaDB.ReadXowaPage(basicName);
			if (xowapage == null) {
				return null;
			}

			string originalTitle = xowapage.title;
			string title = originalTitle ; // may be replaced if it is a redirect

			bool isRedir = xowapage.IsRedirect();
			if (isRedir) {
				// TODO: Check for "redirects to broader term" (redirects with possibilities) template

				string rto = xowapage.RedirectsTo();
				var rpage = xowaDB.ReadXowaPage(rto);
				if (rpage != null) {
					title = rpage.title;

					if (title == bitri.genus) {
						Console.Error.WriteLine("Note: '{0}' redirects to its genus '{1}', so not using as common name.", bitri.FullName(), title);
						return null;
					}

					if (taxonEndings.Any(suffix => title.EndsWith(suffix))) {
						//TODO: option to supress warning
						//TODO: option to ignore
						Console.Write("Note: '{0}' redirects to '{1}', which looks like it's another scenitific name, so not using for common name", bitri.FullName(), title);
						return null;
					}
				}
			}

			if (bitri.isTrinomial) {
				string binom = bitri.ShortBinomial();
				if (title == binom) {
					Console.Error.WriteLine("Note: '{0}' isn't even the full trinomial of '{1}', so not even close to a common name", bitri.FullName(), title);
					return null;
				}

				if (title.StartsWith("Subspecies of ")) {
					Console.Error.WriteLine("Note: '{0}' redirects to '{1}', which looks suspicious", bitri.FullName(), title);
					return null;
				}

				string binomPageTitle = PageNameInWiki(binom);
				if (binomPageTitle != null && binomPageTitle == title) {
					// trinomial redirects to same page as binomial.
					// Assume there are no subsp which are synonymous with their species
					Console.Error.WriteLine("Note: '{0}' redirects to the same page as the binomial '{1}', so not used as common name.", bitri.FullName(), title);
					return null;
				}
			}

			//TODO: check for redirects to genus
			//TODO: check if not redirected to another binom
			//TODO: check if not redirected to a more general taxon

			return title; // may have underscores?? (i think they're removed)

		}

		public string TaxaCommonNameFromWiki(string taxa) {
			// attempts to guess the get the common name from a taxa name
			// returns null if not found, or is dubious

			var xowapage = xowaDB.ReadXowaPage(taxa);
			if (xowapage == null) {
				return null;
			}

			bool isRedir = xowapage.IsRedirect();
			if (isRedir) {
				// TODO: Check for "redirects to broader term" (redirects with possibilities) template
				// 

				string rto = xowapage.RedirectsTo();
				var rpage = xowaDB.ReadXowaPage(rto);
				if (rpage != null) {
					string title = rpage.title;
					if (taxonEndings.Any(suffix => title.EndsWith(suffix))) {
						//TODO: option to supress warning
						Console.Write("Not using Wikipedia for common name of '{0}' because it looks like it's another scenitific name '{1}'", taxa, title);
						return null;
					} else {
						return title;
					}
				}
			} 

			return xowapage.title; // may have underscores?? (i think they're removed)
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

			return xowapage.title; // may have underscores?? (i think they're removed)
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

