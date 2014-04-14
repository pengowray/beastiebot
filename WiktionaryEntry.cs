/* 
			 * 
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


using System;
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
	}
}

