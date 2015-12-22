using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class Options
	{

		[VerbOption("mysqld", HelpText = "Find and launch the Catalogue of Life MySQL daemon.")]
		public MysqldSubOptions MysqldVerb { get; set; }

		[VerbOption("build-species-table", HelpText = "Materialize a list of species from CoL data in the 'beastie' database.")]
		public CommonSubOptions BuildSpeciesTable { get; set; }

		[VerbOption("filter-2gram-species", HelpText = "Extract species from Google ngram (2gram) data.")]
		public FilterNgramSpeciesOptions FilterNgramSpecies { get; set; }

		[VerbOption("tally-species", HelpText = "Tally species from col-species-in-eng-all-2gram-20120701")]
		public TallyListOptions TallySpecies { get; set; }

        [VerbOption("wiki-missing-species", HelpText = "Generate lists of missing species for Wiktionary and Wikipedia")]
        public CommonSubOptions WikiMissingSpeciesAll { get; set; }

        [VerbOption("wiki-missing-species", HelpText = "Tally species from col-species-in-eng-all-2gram-20120701")]
		public TallyListOptions WikiMissingSpecies { get; set; }

		[VerbOption("tally-epithets", HelpText = "Create lists ")]
		public TallyListOptions WikilistSpecies { get; set; }

		[VerbOption("dev", HelpText = "Try the current thing we're working on")]
		public CommonSubOptions DevTest { get; set; }

		[VerbOption("wikipedia-pages-import", HelpText = "Import Wikipedia database files into the database")]
		public CommonSubOptions WikipediaPagesImport { get; set; }

		[VerbOption("wikipedia-redlist", HelpText = "List IUCN critically endangered species for Wikipedia")]
		public CommonSubOptions WikipediaRedlist { get; set; }

		[VerbOption("percent-complete", HelpText = "Statistics for how much is complete.")]
		public CommonSubOptions PercentComplete { get; set; }

		[VerbOption("desc", HelpText="List descendents of an epithet for Wiktionary")]
		public DescSubOptions Descendants { get; set; }

		[VerbOption("get-gni", HelpText="Download GNI database")]
		public CommonSubOptions GniDownload { get; set; }

		//[VerbOption("push", HelpText = "Update remote refs along with associated objects.")]
		//public PushOptions AddVerb { get; set; }

		//[VerbOption("tag", HelpText = "Update remote refs along with associated objects.")]
		//public TagSubOptions TagVerb { get; set; }

		public Options() {
			// Since we create this instance the parser will not overwrite it
			MysqldVerb = new MysqldSubOptions {};
			BuildSpeciesTable = new CommonSubOptions {};
		}

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage() {
			return HelpText.AutoBuild(this,
				(HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}

		[HelpVerbOption]
		public string GetUsage(string verb)
		{
			return HelpText.AutoBuild(this, verb);
		}

	}
}

