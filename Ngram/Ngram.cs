using System;

namespace beastie {
	public struct Ngram {
		public string line;
		public Lemma lemma;
		public int year;
		public long match_count;
		public long volume_count;

		public Ngram(string line) {
			this.line = line;

			var parts = line.Split(new char[]{'\t'});

			string rawLemma = parts[0];
			lemma = new Lemma(rawLemma);

			//CleanLemma(rawLemma);
			//string stem = Stem(Lower(rawLemma));

			year = 0;
			match_count = 0;
			volume_count = 0;

			if (parts.Length < 4) {
				//Console.Error.Write("Bad line: " + line);
				return;
			}

			int.TryParse(parts[1], out year);
			long.TryParse(parts[2], out match_count);
			long.TryParse(parts[3], out volume_count);
		}
	}
}

