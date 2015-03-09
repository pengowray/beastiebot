using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class EpithetTally : NgramReader {

		private Dictionary<string, long> volumeCount = new Dictionary<string, long> ();

		//public int startYear = 0; // all
		public int startYear = 1950; // 1950;

		//TODO TODO TODO: add all these to appropriate SubOptions
		public string kingdom = null; // filter to only use this kingdom // Plantae, Animalia, Bacteria, Fungi, Protozoa (others?)
		public bool onlyCountMissingWiktionary; // set by --onlyNeedingWikiArticle
		bool quickSearch = true;  // true for local search only. much faster. especially good if you've just updated xowa's	db.

		public EpithetTally() {
		}

		protected override void ProcessLine(string line) {
			//example line:
			//Dicerorhinus sumatrensis	2005	69	34
			//lemma TAB year TAB match_count TAB volume_count

			Ngram ngram = new Ngram(line);

			if (ngram.year <= startYear) {
				return;
			}

			string species = ngram.lemma.raw;

			if (volumeCount.ContainsKey(species)) {
				volumeCount[species] += ngram.volume_count;
			} else {
				volumeCount[species] = ngram.volume_count;
			}
		}

		public void OutputEpithetCountsToFile(string filename) {
			string speciesSetFile = FileConfig.Instance().colSpeciesListFile;
			//bool onlyCountMissingWiktionary = true;
			int maxEntries = 5000; // -1 (or 0) for all. //TODO: make an option
			bool kingdomFilterOn = !string.IsNullOrEmpty(kingdom);

			//var sorted = from entry in volumeCount orderby entry.Value descending select entry;
			var stemBalls = new Dictionary<string, LatinStemBall>();
			WiktionaryBot wikt = null;
			if (onlyCountMissingWiktionary) {
				wikt = WiktionaryBot.Instance();
			}
			foreach (var sp in volumeCount) {
				var species = new Species(sp.Key);
				string stem = LatinStemmer.stemAsNoun(species.epithet);
				if (string.IsNullOrEmpty(stem))
					continue;
					
				if (kingdomFilterOn) {
					SpeciesDetails details = new SpeciesDetails(species);
					details.Load();
					if (details.kingdom != null && details.kingdom != kingdom)
						continue;
				}

				if (! stemBalls.ContainsKey(stem)) {
					stemBalls[stem] = new LatinStemBall();
				}
				if (onlyCountMissingWiktionary) {
					bool missing = !wikt.ExistsMulLa(species.epithet, quickSearch);
					Console.WriteLine(species.epithet + " found? " + !missing);
					stemBalls[stem].Add(species, sp.Value, missing);
				} else {
					stemBalls[stem].Add(species, sp.Value);
				}
			}

			// TODO: implement the following when kingdom filter is on too.
			if (!kingdomFilterOn) {

				// add a little for each species
				if (speciesSetFile != null) {
					SpeciesSet speciesSet = new SpeciesSet();
					speciesSet.ReadCsv(speciesSetFile);

					// species.. 1 count to each epithet for each species...

					foreach (Species sp in speciesSet.AllSpecies()) {
						string epStem = LatinStemmer.stemAsNoun(sp.epithet);
						if (!stemBalls.ContainsKey(epStem)) {
							stemBalls[epStem] = new LatinStemBall();
						}
						if (onlyCountMissingWiktionary) {
							// TODO: give weight if they're missing.. do only wikt database search and skip online search
							stemBalls[epStem].Add(sp, 1, false);
						} else {
							stemBalls[epStem].Add(sp, 1);
						}
					}

					// genera (genus) (must be done after species)
					foreach (Species sp in speciesSet.AllSpecies()) {
						string genusStem = LatinStemmer.stemAsNoun(sp.genus);
						if (stemBalls.ContainsKey(genusStem)) {
							stemBalls[genusStem].AddGenus(sp.genus, 1);
						}
					}
				}

				// increase genus counts according to their volume counts
				foreach (var sp in volumeCount) {
					var species = new Species(sp.Key);
					string stem = LatinStemmer.stemAsNoun(species.genus);
					if (stemBalls.ContainsKey(stem)) {
						stemBalls[stem].AddGenus(species.genus, sp.Value);
					}
				}

				//AddOtherScientificNameElements(stemBalls);
			}
			FindWhatIsStillInUse(stemBalls);

			//TODO FIXME XXXXXXXXX put this back and make an option
			//var sorted = from entry in stemBalls.Values orderby entry.total descending select entry;
			//var sorted = from entry in stemBalls.Values orderby entry.FirstDeclScore() descending select entry;
			var sorted = from entry in stemBalls.Values where entry.FeminineScore() > 0 orderby entry.FeminineScore() descending select entry;

			int count = 0;
			int headingEvery = 10;
			using (var output = new StreamWriter(filename, false, Encoding.UTF8)) {
				foreach (LatinStemBall sb in sorted) {
					if (maxEntries > 0 && count >= maxEntries)
						break;

					if (count % headingEvery == 0) {
						output.WriteLine(string.Format("=={0}–{1}==", count + 1, count + headingEvery));
					}

					//TODO: onlyNeedingWikiArticle (or onlyNeedingWiktEntry (no mul/la)

					string genList; 
					if (kingdomFilterOn) {
						genList = sb.PrettyGenusListNoNumbers();
					} else {
						genList = sb.PrettyGenusList();
					}

					if (genList.Length > 2) {
						output.WriteLine("# {0} — {1}", sb.PrettyPrint(), genList); 
					} else {
						output.WriteLine("# {0}", sb.PrettyPrint()); 
					}

					output.WriteLine("#: e.g. {0}", sb.PrettyExamples());
					if (sb.Descendants() != null) {
						output.WriteLine(sb.Descendants());
					}

					count++;
				}
			}

		}

		void FindWhatIsStillInUse(Dictionary<string, LatinStemBall> stemBalls) {
			string scientific_name_elements = @"D:\Dropbox\latin2-more\beastierank\output\scientific_name_elements.csv";
			string scientific_name_elements_synonyms = @"D:\Dropbox\latin2-more\beastierank\output\scientific_name_elements_synonyms.csv";

			// must do synonyms first.

			using (var infile = new StreamReader(scientific_name_elements_synonyms, Encoding.UTF8, true)) {

				CsvReader csv = new CsvReader(new StreamReader(scientific_name_elements), true);

				string[] headers = csv.GetFieldHeaders();
				if (csv.FieldCount != 4) {
					//TODO: if one field, then treat as space-separated (using first space)
					throw new Exception(string.Format("FindWhatIsStillInUse() found wrong number of fields. Found: {0}", headers.Length));
				}

				while (csv.ReadNextRecord())
				{
					string term = csv[0]; // note: all lowercase (even genus)
					string rank = csv[1];

					string stem = LatinStemmer.stemAsNoun(term);
					if (string.IsNullOrEmpty(stem))
						continue;

					if (!stemBalls.ContainsKey(stem))
						continue;

					if (rank == "species" || rank == "subspecies") {
						stemBalls[stem].StillUsed(term);

					} else if (rank == "genus") {
						// uppercase first letter
						term = term.TitleCaseOneWord();

						stemBalls[stem].StillUsed(term);

					} else {
						// ignore other ranks for now
					}

					//long i = csv.CurrentRecordIndex;
					//stemBalls.Add(new Species(csv[0], csv[1]));


				}


			}
		}



		//-- scientific_name_status_id = 1 (accepted), 2=ambiguous syn, 3=misapplied name, 4=provisionally accepted name, 5=synonym



	}
}
