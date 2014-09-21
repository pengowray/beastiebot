using System;
using CommandLine;
using CommandLine.Text;

namespace beastie {
	class CommonSubOptions { // abstract
		//[Option('q', "quiet", HelpText = "Suppress summary message.")]
		//public bool Quiet { get; set; }

		//TODO: move to a COL specific SubOptions?
		[Option('n', "no-run-mysqld", HelpText = "Dont attempt to start CatalogueOfLife's mysqld, even if it's needed.")]
		public bool DontRunNewMysqld { get; set; }

		[Option('F', "force", HelpText = "Force action even when existing database table or output file exists.")]
		public bool Force { get; set; }

	}
}

