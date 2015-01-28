using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class TallyListOptions : CommonSubOptions
	{
		[Option('o', "output", HelpText = "Output file")]
		public string outputFile { get; set; }

		[Option('l', "species-ngram-file", HelpText = "ngram (2gram) file which contains just species. i.e. the output of filter-2gram-species.")]
		public string speciesNgramFile { get; set; }

		[Option('w', "wiki-style-output", HelpText = "If true, output in a format Wiki-format style. If false, output is CSV.")]
		public bool wikiStyleOutput { get; set; }

		[Option('k', "kingdom", HelpText = "Only include species of this kingdom: Plantae, Animalia, Bacteria, Fungi, Protozoa  (must use correct capitalization)")]
		public string kingdom { get; set; }

		[Option('T', "todo", HelpText = "Only include items which require a Wikipedia article")]
		public bool onlyNeedingWikiArticle { get; set; }

		[Option('s', "since", HelpText = "Tally for ranking only counts book published after this date (default: 1950)")]
		public int? since { get; set; }

	}
}
