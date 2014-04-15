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
	public class WiktionaryEntry
	{
		public string title;
		public long id;
		public long parentid;
		public int ns;
		//todo: date timestamp;
		//todo: contributor
		//todo: comment
		public string text; // main text
		public string sha1;
		//public string model;
		//public string format;

		public WiktionaryEntry () {
		}

		public Dictionary<string,string> Sections() { // heading => text
			// TODO: everything the parser does: https://www.mediawiki.org/wiki/Markup_spec#Parser_outline
			// See also: D:\ngrams\mediawiki-source\mediawiki-1.22.5\includes\parser\parser.php

			//WARNING: sanitize output if creating HTML

			// e.g. "English" => english part of entry
			// "top" => before any section

			//StringReader strReader = new StringReader(textReaderText);
			//aLine = strReader.ReadLine();

			Dictionary<string,string> sections = new Dictionary<string,string>();

			string text = this.text;

			// remove comments <!-- x -->
			text = Regex.Replace(text, "<!--.*?-->", String.Empty, RegexOptions.Singleline);
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


			return sections;

		}
	}
}

