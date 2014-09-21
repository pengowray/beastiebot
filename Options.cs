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

