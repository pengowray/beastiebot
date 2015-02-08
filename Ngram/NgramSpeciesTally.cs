using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class NgramSpeciesTally : NgramReader {

		private Dictionary<string, long> volumeCount = new Dictionary<string, long> ();

		//public int startYear = 0; // all
		public int startYear = 1950;

		public string kingdom = null; // filter to only use this kingdom // Plantae, Animalia, Bacteria, Fungi, Protozoa (any others?)
		public string class_ = null; // e.g. Insecta

		//TODO TODO TODO: add all these to appropriate SubOptions
		public bool onlyNeedingWikiArticle = false; // if true, only show those needing a wikipedia article.
		public bool onlyCountMissingWiktionary = false;
		bool quickSearch = true;  // true for local search only. much faster. especially good if you've just updated xowa's	db.

		public NgramSpeciesTally() {
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

		public void OutputToFile(string filename) {
			var sorted = from entry in volumeCount orderby entry.Value descending select entry;

			if (filename == null || filename == "") {
				foreach (var sp in sorted) {
					Console.WriteLine("{0},{1}", sp.Key, sp.Value);
				}
			} else {
				var output = new StreamWriter(filename, false, Encoding.UTF8);
				foreach (var sp in sorted) {
					output.WriteLine("{0},{1}", sp.Key, sp.Value);
				}
				output.Close();
			}

			Console.WriteLine("Species: {0}", volumeCount.Count); 
		}

		public void OutputEpithetCountsToFile(string filename, string speciesSetFile = null) {
			//bool onlyCountMissingWiktionary = true;
			int maxEntries = 5000; // -1 (or 0) for all.
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
					Console.WriteLine(species.epithet + " missing? " + missing);
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

			//var sorted = from entry in stemBalls orderby entry.Value.total descending select entry.Value;
			var sorted = from entry in stemBalls.Values orderby entry.total descending select entry;

			int count = 0;
			int headingEvery = 50;
			using (var output = new StreamWriter(filename, false, Encoding.UTF8)) {
				foreach (var sb in sorted) {
					if (maxEntries > 0 && count >= maxEntries)
						break;

					if (count % 50 == 0) {
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

		public void WikiListToFile(string filename) {
			var sorted = from entry in volumeCount orderby entry.Value descending select entry;

			using (var output = new StreamWriter(filename, false, Encoding.UTF8)) {
				foreach (var spEntry in sorted) {
					var species = new SpeciesDetails(spEntry.Key);
					species.Load();
					bool kingdomFilterOn = !string.IsNullOrEmpty(kingdom);
					bool classFilterOn = !string.IsNullOrEmpty(class_);
					if (kingdomFilterOn && kingdom != species.kingdom) {
						continue; // failed to match
					}
					if (classFilterOn && class_ != species.class_) {
						continue; // failed to match
					}

					if (onlyNeedingWikiArticle) {
						if (!species.NeedsEnWikiArticle())
							continue;
					}

					output.WriteLine("# {0}", PrettyPrintSpecies(species, kingdomFilterOn, classFilterOn)); 
					output.Flush();
				}
			}


		}

		private static string PrettyPrintSpecies(SpeciesDetails species, bool kingdomFilterOn, bool classFilterOn) {
			if (species == null)
				return "(not found)";

			if (species.isAccepted) {
				string commonName = species.MostEnglishName();
				string monotypic = species.isMonotypic() ? " (monotypic)" : "";
				string commonNameText = string.IsNullOrWhiteSpace(commonName) ? "" 
					: string.Format(" - [[{0}]]", commonName.ToLowerInvariant()); 
				string kingdomPhylum;
				if (classFilterOn) {
					kingdomPhylum = species.PrettyOrderFamily();
				} else if (kingdomFilterOn) {
					kingdomPhylum = species.PrettyPhylumClass();
				} else {
					kingdomPhylum = species.PrettyKingdomPhylum();
				}

				return string.Format("''[[{0}]]'' — ''[[{1}]]''{2}{3} {4}", species.species, species.species.genus, monotypic, commonNameText, kingdomPhylum);

			} else if (species.status == Status.not_found) {
				if (string.IsNullOrWhiteSpace(species.species.ToString())) {
					return "(unknown)";
				} else {
					return string.Format("''[[{0}]]'' (not found)", species.species);
				}

			} else {
				var accepted = species.AcceptedSpeciesDetails();
				if (accepted != null) {
					accepted.Load();
					return string.Format("''[[{0}]]'' ({1}) = {2}", // e.g. "(synonym) ="
						species.species, 
						species.status, 
						PrettyPrintSpecies(accepted, kingdomFilterOn, classFilterOn));
				} else {
					return string.Format("''[[{0}]]'' ({1}).", 
						species.species, 
						species.status);

				}
			}


		}
	}
}
