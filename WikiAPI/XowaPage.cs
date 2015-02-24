using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace beastie {
	/// <summary>
	/// Modelled on DotNetWikiBot's Page
	/// </summary>
	public class XowaPage
	{

		static Dictionary<string, DotNetWikiBot.Site> wikiBotSites = new Dictionary<string, DotNetWikiBot.Site>();

		private static Dictionary<string,Regex> regexes = null;

		static XowaPage() {
			// code taken from DotNetWikiBot: Site.Initialize()

			regexes = new Dictionary<string,Regex>();
			regexes["titleLink"] =
				new Regex("<a [^>]*title=\"(?<title>.+?)\"");
			regexes["titleLinkInList"] =
				new Regex("<li(?: [^>]*)?>\\s*<a [^>]*title=\"(?<title>.+?)\"");
			regexes["titleLinkInTable"] =
				new Regex("<td(?: [^>]*)?>\\s*<a [^>]*title=\"(?<title>.+?)\"");
			regexes["titleLinkShown"] =
				new Regex("<a [^>]*title=\"([^\"]+)\"[^>]*>\\s*\\1\\s*</a>");
			regexes["linkToSubCategory"] =
				new Regex(">([^<]+)</a></div>\\s*<div class=\"CategoryTreeChildren\"");
			regexes["linkToImage"] =
				new Regex("<div class=\"gallerytext\">\n<a href=\"[^\"]*?\" title=\"([^\"]+?)\">");
			regexes["wikiLink"] =
				new Regex(@"\[\[(?<link>(?<title>.+?)(?<params>\|.+?)?)]]");
			regexes["wikiTemplate"] =
				new Regex(@"(?s)\{\{(.+?)((\|.*?)*?)}}");
			regexes["webLink"] =
				new Regex("(https?|t?ftp|news|nntp|telnet|irc|gopher)://([^\\s'\"<>]+)");
			regexes["noWikiMarkup"] =
				new Regex("(?is)<nowiki>(.*?)</nowiki>");
			regexes["editToken"] =
				new Regex("(?i)value=\"([^\"]+)\"[^>]+name=\"wpEditToken\"" +
			"|name=\"wpEditToken\"[^>]+value=\"([^\"]+)\"");

			// in the original code, this is usually loaded dynamically from the site or something
			regexes["redirect"] = new Regex(@"(?i)^ *#(?:REDIRECT)\s*:?\s*\[\[(.+?)(\|.+)?]]");

			//regexes["redirect"] = new Regex(@"(?i)^ *#(?:" + generalData["redirectTags"] +
			// @")\s*:?\s*\[\[(.+?)(\|.+)?]]");


		}


		int ns;

		/// <summary>Page's title, including namespace prefix (or doesn't because it's from xowa).</summary>
		public string title;
		/// <summary>Page's text.</summary>
		public string text;
		/// <summary>Site, on which this page is located. (e.g. "en.wikipedia.org")</summary>
		public string siteDomain;
		/// <summary>Page's ID in MediaWiki database.</summary>
		public string pageId;
		/// <summary>Date and time of last edit expressed in UTC (Coordinated Universal Time).
		/// Call "timestamp.ToLocalTime()" to convert to local time if it is necessary.</summary>
		public DateTime timestamp;

		/// <summary>Returns true, if page redirects to another page. Don't forget to load
		/// actual page contents from live wiki "Page.Load()" before using this function.</summary>
		/// <returns>Returns bool value.</returns>
		public bool IsRedirect()
		{
			if (text == null)
				return false;

			return regexes["redirect"].IsMatch(text);
		}

		public bool xowa_redirect; // does xowa db see it as a redirect

		/// <summary>Returns redirection target. 
		/// <returns>Returns redirection target page title as string. Or empty string, if this
		/// Page object does not redirect anywhere.</returns>
		public string RedirectsTo()
		{
			if (IsRedirect())
				return regexes["redirect"].Match(text).Groups[1].ToString().Trim();
			else
				return string.Empty;
		}

		public XowaPage() {
		}

		public DotNetWikiBot.Page ToPage() {
			DotNetWikiBot.Bot.cacheDir = @"C:\Cache"; //TODO: move this somewhere and/or make configurable

			if (!wikiBotSites.ContainsKey(siteDomain)) {
				wikiBotSites[siteDomain] = new DotNetWikiBot.Site(siteDomain);
			}

			var page = new DotNetWikiBot.Page(wikiBotSites[siteDomain], title); // underscores will be changed to spaces
			page.text = text;
			page.title = title;
			page.pageId = pageId;
			page.timestamp = timestamp;

			return page;
		}

		public void DebugPrint() {
			Console.WriteLine("{4} {0} (xow-redir:{1}|regex-redir:{2}|{5}): {3}", title, xowa_redirect, IsRedirect(), text, pageId, RedirectsTo());
		}
	}

}

