using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class DescSubOptions : CommonSubOptions
	{
		//TODO
		[Option('o', "output", HelpText = "Output file")]
		public string outputFile { get; set; }

		//moved to FileConfig
		//[Option('l', "species-list", HelpText = "Species list (csv: genus,epithet)")]
		//public string speciesList { get; set; }

		[Option('t', "epithet", HelpText = "The specific epithet (or specific name) to get the descendents of (requred)")]
		public string epithet { get; set; }

		[Option('r', "rigorous", HelpText = "Actually check Wiktionary for missing entries. Don't rely only on Xowa database.")]
		public bool rigorous { get; set; }

	}
}

