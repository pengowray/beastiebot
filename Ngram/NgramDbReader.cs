using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//TODO: read book (via Cliff, Democracy guy): the advertised mind, seducing the subconscious
//TODO: create vocabprimer.com

// creates a database table with the match and volume counts from 1950.
// match counts use cleaned lemmas (e.g. "afternoon.we" counts towards "afternoon")
// volume counts only use the cannonical form (non-cannoncial/uncleaned forms are discarded)

namespace beastie
{
	public class NgramDbReader : NgramReader
	{
		NgramDatabase ngramDatabase;

		string corpus = "unknown";
		int startYear = 1950;

		public NgramDbReader (string corpus, int startYear = 1950) {
			this.corpus = corpus;
			this.startYear = startYear;
		}

		protected override void Start() {
			ngramDatabase = new NgramDatabase();
			ngramDatabase.CreateTables();
		}

		protected override void End() {
			ngramDatabase.Dispose();
		}

		protected override void ProcessLine(string line) {

			Ngram ngram = new Ngram(line);

			if (ngram.year < startYear)
				return;

			long matchAdd = 0;
			long volumeAdd = 0;

			if (! ngram.lemma.hasPos) matchAdd = ngram.match_count;
			if (ngram.lemma.isCanonical) volumeAdd = ngram.volume_count;

			if (volumeAdd > 0 || matchAdd > 0) {
				ngramDatabase.AddLemmaCounts(ngram.lemma.cleaned, corpus, matchAdd, volumeAdd);
			}
		}
	}
}

