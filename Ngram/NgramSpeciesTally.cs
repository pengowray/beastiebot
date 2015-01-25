using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace beastie {
	public class NgramSpeciesTally : NgramReader {

		private Dictionary<string, long> volumeCount = new Dictionary<string, long> ();

		//public int startYear = 0; // all
		public int startYear = 1950;

		public string kingdom = null; // filter to only use this kingdom // Plantae, Animalia, Bacteria, Fungi, Protozoa (any others?)

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

		//-- scientific_name_status_id = 1 (accepted), 2=ambiguous syn, 3=misapplied name, 4=provisionally accepted name, 5=synonym

		public void WikiListToFile(string filename) {
			var sorted = from entry in volumeCount orderby entry.Value descending select entry;

			using (var output = new StreamWriter(filename, false, Encoding.UTF8)) {
				foreach (var spEntry in sorted) {
					var species = new SpeciesDetails(spEntry.Key);
					species.Query();
					bool kingdomFilterOn = !string.IsNullOrEmpty(kingdom);
					if (kingdomFilterOn && kingdom != species.kingdom) {
						// failed to match
						continue;
					}
					output.WriteLine("# {0}", PrettyPrintSpecies(species, kingdomFilterOn)); 
				}
			}


		}

		private static string PrettyPrintSpecies(SpeciesDetails species, bool kingdomFilterOn) {
			if (species == null)
				return "(not found)";

			if (species.isAccepted) {
				string commonName = species.MostEnglishName();
				string monotypic = species.isMonotypic() ? " (monotypic)" : "";
				string commonNameText = string.IsNullOrWhiteSpace(commonName) ? "" 
					: string.Format(" - [[{0}]]", commonName.ToLowerInvariant()); 
				string kingdomPhylum = kingdomFilterOn ? species.PrettyPhylumClass() : species.PrettyKingdomPhylum();

				return string.Format("''[[{0}]]'', ''[[{1}]]''{2}{3} {4}", species.species, species.species.genus, monotypic, commonNameText, kingdomPhylum);

			} else if (species.status == Status.not_found) {
				return string.Format("''[[{0}]]'' (not found)", species.species);

			} else {
				var accepted = species.AcceptedSpeciesDetails();
				if (accepted != null) {
					accepted.Query();
					return string.Format("''[[{0}]]'' ({1}) = {2}", 
						species.species, 
						species.status, 
						PrettyPrintSpecies(accepted, kingdomFilterOn));
				} else {
					return string.Format("''[[{0}]]'' ({1}).", 
						species.species, 
						species.status);

				}
			}


		}
	}
}
