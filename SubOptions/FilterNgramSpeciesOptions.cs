using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class FilterNgramSpeciesOptions : CommonSubOptions
	{
		[Option('o', "output", HelpText = "Output file")]
		public string outputFile { get; set; }

		//[Option('l', "species-list", HelpText = "Species list (csv: genus,epithet)")]
		//public string speciesList { get; set; }

		[Option("append", HelpText = "Append to output file.")]
		public bool append { get; set; }

	}
}

