using System;

namespace beastie {
	public class FileConfig {
        //http://www.iucnredlist.org/about/summary-statistics#Table_9
        public static string datadir = @"C:\ngrams\";
        public static string coredata = @"C:\dropbox\latin2-more\beastierank\output\";

		// used by tally-species, wiki-missing-species, tally-epithets
		//[Option('l', "species-ngram-file", HelpText = "ngram (2gram) file which contains just species. i.e. the output of filter-2gram-species.")]
		//public string speciesNgramFile { get; set; }
		public string speciesNgramFile = datadir + @"datasets-generated\col-species-in-eng-all-2gram-20120701.txt";

		//[Option('l', "species-list", HelpText = "Species list (csv: genus,epithet)")]
		public string colSpeciesListFile = coredata + @"all species and synonyms CoL2014.csv";

		public string gniDownloadFile = datadir + @"datasets-gni\gni-download.csv.txt";

        //public string iucnRedListFile = datadir + @"datasets-iucn\2014.3\2015-02-09_Everything-but-regional-export-57234.csv\export-57234.csv";
        public string iucnRedListFile = datadir + @"datasets-iucn\2015.4\export-57234-03_December_2015-all_but_regional.csv";
        // via http://www.iucnredlist.org/about/summary-statistics (copy paste pdf's text and then cleaned up by hand)
        //public string iucnPossiblyExtinctFile = datadir + @"datasets-iucn\2014.3\possiblyextinct.txt";
        public string iucnPossiblyExtinctFile = datadir + @"datasets-iucn\2015.4\possiblyextinct_19_November_2015.txt";

        public string GNISafeEscapedCSV = datadir + @"datasets-gni\gni-merged-safe-escaped.csv.txt";

		FileConfig() {
		}

		private static FileConfig _instance;
		public static FileConfig Instance() {
			if (_instance == null) {
				_instance = new FileConfig();
			}
			return _instance;
		}
	}
}

