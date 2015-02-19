using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace beastie {
	public class WikiMissingSpecies : NgramReader {

		private Dictionary<string, long> volumeCount = new Dictionary<string, long> ();

		//public int startYear = 0; // all
		public int startYear = 1950; // 1950;

		//TODO TODO TODO: add all these to appropriate SubOptions
		public string kingdom = null; // filter to only use this kingdom // Plantae, Animalia, Bacteria, Fungi, Protozoa (any others?)
		public string class_ = null; // e.g. Insecta
		public bool onlyNeedingWikiArticle = false; // if true, only show those needing a wikipedia article.

		public WikiMissingSpecies() {
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
