using System;

namespace beastie {

	//TODO: this file isn't used yet

	//TODO: create .ini file for setting locations of main files, databases, filename schemes, etc.


	public class Config {
		public string generated_data = @"D:\ngrams\datasets-generated\";

		public string speciesFile = @"D:\Dropbox\latin2-more\beastierank\output\all species and synonyms CoL2014.csv";

		public string google_books_2grams_eng_all = @"http://storage.googleapis.com/books/ngrams/books/googlebooks-eng-all-2gram-20120701-{0}.gz";

		public string blah = @"col-species-in-eng-all-2gram-20120701.txt";

		public Config() {
		}
	}
}

