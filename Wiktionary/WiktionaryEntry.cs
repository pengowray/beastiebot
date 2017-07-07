using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/* 

<page>
	<title>
	bun
	</title>
	<ns>
	0
	</ns>
	<id>
	47394
	</id>
	<revision>
		<id>
		25876395
		</id>
		<parentid>
		25830404
		</parentid>
		<timestamp>
		2014-03-25T14:25:46Z
		</timestamp>
		<contributor>
			<ip>
			212.99.21.163
			</ip>
		</contributor>
		<comment>
		t+eo:[[bulko]] ([[WT:EDIT|Assisted]])
		</comment>
		<text xml:space='preserve'>
		{{also|bún|bùn|bűn|bûn|bün|BUN}}
		==English==
		...
		</text>
		<sha1>
		7rnent3dh1jhz8dqtv9eoge4ai2jynx
		</sha1>
		<model>
		wikitext
		</model>
		<format>
		text/x-wiki
		</format>
	</revision>
</page>
*/

namespace beastie {
	public class WiktionaryEntry {
		public string title;
		public string id; // long id
		public long parentid;
		public int ns;
		//todo: date timestamp;
		//todo: contributor
		//todo: comment
		public string text; // main text
		public string sha1;
		//public string model;
		//public string format;


		private Dictionary<string,string> sections;

		public WiktionaryEntry () {
		}

		public Dictionary<string,string> Sections() { // heading => text
			if (this.sections != null) return sections;

            // Note: Section text has comments removed.

            // TODO: <nowiki> support

            // TODO: everything the parser does: https://www.mediawiki.org/wiki/Markup_spec#Parser_outline
            // See also: C:\ngrams\mediawiki-source\mediawiki-1.22.5\includes\parser\parser.php
            // See also: https://github.com/wikimedia/mediawiki/blob/master/tests/parser/parserTests.txt

            //WARNING: sanitize output if creating HTML

            // e.g. "English" => english part of entry
            // "top" => before any section

            //StringReader strReader = new StringReader(textReaderText);
            //aLine = strReader.ReadLine();

            sections = new Dictionary<string,string>();

			string text = this.text;

			if (text == null) return sections;

			// remove comments <!-- x -->
			text = Regex.Replace(text, "<!--.*?-->", String.Empty, RegexOptions.Singleline);

			// break up headings
			//TODO: should probably just capture the start and end of substring and only actually call Substring() when requested.
			string regexSearch = @"^==\s*([^=].+?)\s*==\s*$"; // find only ==h2 headers==... // @"^==([^=].+[^=])==\s*$" to be more strict against: ==header===
			int previousMatchEnd = 0;
			string heading = null;
			foreach (Match match in Regex.Matches(text, regexSearch, RegexOptions.Multiline)) {
				string prevText = text.Substring(previousMatchEnd, match.Index - previousMatchEnd);
				sections[heading ?? "top"] = prevText;

				heading = match.Groups[1].Captures[0].Value; // e.g. "English"
				previousMatchEnd = match.Index + match.Length;

				//Console.WriteLine("Found '{0}' at position {1} capture {2} length {3}", match.Value, match.Index, match.Groups[1].Captures[0], match.Length);
				//Console.WriteLine("Heading: '{0}'", heading);
			}
			string finalText = text.Substring(previousMatchEnd);
			sections[heading ?? "top"] = finalText;

			//$text = preg_replace( @"(^|\n)-----*", @"\1<hr />", $text );


			for (int i = 6; i >= 1; --i ) {
				string h = new string('=', i);
				//string regexSearch = string.Format( @"/^{0}(.+){0}\s*$/m", h );
				//string regexReplace = string.Format( @"<h{0}>\1</h{0}>", i );
				//text = preg_replace( @"/^$h(.+)$h\s*$/m", @"<h$i>\1</h$i>", $text );
			}

			//https://en.wiktionary.org/wiki/Unsupported_titles/Number_sign
			//{{also|♯}}{{unsupportedpage|#}}
			//https://en.wiktionary.org/wiki/Unsupported_titles/Ideographic_space
			//{{CJK Symbols and Punctuation character info|hex=3000|name=IDEOGRAPHIC SPACE}}

			return sections;
		}

		public static string[] TemplateList(string text) {
			//string text = Sections()[lang]; // section

			//TODO: <nowiki> support
			//TODO: <!-- comment support --> (though already stripped out)

			//string nextToken = @"(\{\{|\}\})"; // {{ or }} (captured)

			int depth = 0;
			int loc = 0;
			int depthOneStart = 0;

			List<string> templates = new List<string>();

			while (loc < text.Length) { // -1? 
				//find a {{
				if (depth == 0) {
					int nextOpen = text.IndexOf("{{", loc);
					if (nextOpen != -1) {
						depth++;
						loc = nextOpen + 2;
						depthOneStart = loc;
					} else {
						loc = text.Length;
					}

				} else {

					int nextOpen = text.IndexOf("{{", loc);
					int nextClose = text.IndexOf("}}", loc);
					string token = "";

					// which token is next (stupidly long way of working it out)
					if (nextOpen == -1 && nextClose == -1) {
						// TODO: warn: closing }} missing. 
						token = ""; // finished
					} else if (nextOpen != -1 && nextClose == -1) { 
						token = "{{";
					} else if (nextOpen == -1 && nextClose != -1) { 
						token = "}}";
					} else {
						if (nextOpen < nextClose) {
							token = "{{";
						} else {
							token = "}}";
						}
					}

					if (token == "{{") {
						loc = nextOpen + 2;
						depth++;
					} else if (token == "}}") {
						if (depth == 1) {
							string template = text.Substring(depthOneStart, nextClose - depthOneStart);
							templates.Add(template);
							Console.WriteLine("template:");
							Console.WriteLine(template);
						}
						loc = nextClose + 2;
						if (depth != 0) depth--;

					} else {
						loc = text.Length;
					}

				}
			}

			return templates.ToArray();
		}





		public static void DefinitionTemplateUsageStats(string text, ref Dictionary<string,int> stats) {
			//Adds one (only) to stats[templateName] if it is found in the text.

			//TODO: multi-line definitions (i.e. with sub-definitions)

			//string heading = @"^[\=]+(.+)[\=]+\s*$"; //TODO: work out which heading the defs are under

			//NOTE: references also sometimes look like definitions, so are included until headings are used to exclude them

			//if (stats == null) stats = new Dictionary<string, int>();

			string definition = @"^\#\s*(?=[^\:^\*^\#])(.*)$"; // line starting with a #, not #:, #*, ##
			string templateName = @"\{\{(.*?)(\||\}\})"; // {{...|, or {{...}}, trim afterwards

			Dictionary<string,int> templCounts = new Dictionary<string, int>();

			//stats["test"] = 1;

			foreach (Match m in Regex.Matches(text, definition, RegexOptions.Multiline)) {
				string defLine = m.Groups[1].Captures[0].Value;
				//Console.WriteLine( defLine );
				foreach (Match m2 in Regex.Matches(defLine, templateName)) {
					string templName = m2.Groups[1].Captures[0].Value.Trim();
					if (templCounts.ContainsKey(templName)) {
						templCounts[templName]++;
						//stats[templName]++; // comment in for total usage, not just number of usages
					} else {
						templCounts[templName] = 1;
						if (stats.ContainsKey(templName)) {
							stats[templName]++;
						} else {
							stats[templName] = 1;
						}
					}
				}
			}

		}
	}
	
}

