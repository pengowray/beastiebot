//using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class MysqldSubOptions : CommonSubOptions
	{
		//TODO: move to a COL specific SubOptions
		[Option('y', "colversion", Required = false, 
			HelpText = "Which version of CatalogueOfLife to look for (defaults to 2014. Not tested with other years/releases.")]
		public string year { get; set; }

		[Option("shutdown", HelpText = "Shutdown mysqld instance")]
		public bool shutdown { get; set; }

	}
}

