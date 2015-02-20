using System;

namespace beastie {
	public class FileConfig {
		// used by tally-species, wiki-missing-species, tally-epithets
		//[Option('l', "species-ngram-file", HelpText = "ngram (2gram) file which contains just species. i.e. the output of filter-2gram-species.")]
		//public string speciesNgramFile { get; set; }
		public string speciesNgramFile = @"D:\ngrams\datasets-generated\col-species-in-eng-all-2gram-20120701.txt";

		//[Option('l', "species-list", HelpText = "Species list (csv: genus,epithet)")]
		public string colSpeciesListFile = @"D:\Dropbox\latin2-more\beastierank\output\all species and synonyms CoL2014.csv";



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

