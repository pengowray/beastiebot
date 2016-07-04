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

        public string mostLatinLooking = coredata + @"ranked_by_latinesqueness_{0}.txt"; // see WordClassifier. 0 = "species"

        public string gniDownloadFile = datadir + @"datasets-gni\gni-download.csv.txt";

        //public string iucnRedListFile = datadir + @"datasets-iucn\2014.3\2015-02-09_Everything-but-regional-export-57234.csv\export-57234.csv";
        //public string iucnRedListFile = datadir + @"datasets-iucn\2015.4\export-57234-03_December_2015-all_but_regional.csv";
        public string iucnRedListFile = datadir + @"datasets-iucn\2015.4b\export-57234-19_February_2016-everything_but_regional.csv";
        public string iucnRedListFileDate = "February 2016";
        public string iucnRedListFileShortDate = "2015.4";
        //HEY!: don't forget to edit date here too:
        public string iucnRedListFileRef = "<ref name=IUCNDATA>{{cite web|title=IUCN Red List version 2015.4|url=http://www.iucnredlist.org/|website=The IUCN Red List of Threatened Species|publisher=International Union for Conservation of Nature and Natural Resources (IUCN)|accessdate=19 February 2016}}</ref>";
        public string iucnRedListFileShortRef = "<ref name=IUCNDATA />";
        public string CommonNameDupesFile = datadir + @"datasets-iucn\dupes_common_name_generated.txt"; // TODO: versioning? database?
        public string WikiAmbigDupesFile = datadir + @"datasets-iucn\dupes_redirects_to_same_title_generated.txt"; // TODO: versioning? database?
        public string WikiDupesReportFile = datadir + @"datasets-iucn\dupes_redirects_to_same_title_public_report.txt"; // TODO: versioning? database?
        public string CapsReportFile = datadir + @"datasets-iucn\caps"; // add .txt or _generated.txt to the end
        public string commonNameIssuesFile = datadir + @"datasets-iucn\common_name_issues.txt";

        // via http://www.iucnredlist.org/about/summary-statistics (copy paste pdf's text and then cleaned up by hand)
        //public string iucnPossiblyExtinctFile = datadir + @"datasets-iucn\2014.3\possiblyextinct.txt";
        public string iucnPossiblyExtinctFile = datadir + @"datasets-iucn\2015.4\possiblyextinct_19_November_2015.txt";
        //HEY!: don't forget to edit date here too:
        public string iucnPossiblyExtinctFileRef = "<ref name=IUCNPE>{{cite web|title=Table 9: Possibly Extinct and Possibly Extinct in the Wild Species (IUCN Red List version 2015.4)|url=http://www.iucnredlist.org/about/summary-statistics|website=The IUCN Red List of Threatened Species|publisher=Union for Conservation of Nature and Natural Resources (IUCN)|accessdate=19 November 2015}}</ref>";
        public string iucnPossiblyExtinctFileShortRef = "<ref name=IUCNPE />";

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

