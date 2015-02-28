	using System;
using System.Text.RegularExpressions;

namespace beastie {
	public class Bioparser
	{
		public Bioparser() {
		}

		//see also: https://github.com/GlobalNamesArchitecture/biodiversity
		// https://github.com/GlobalNamesArchitecture/biodiversity/blob/master/lib/biodiversity/parser.rb

		// Iberis spathulata J.P.Bergeret subsp. lereschiana (Barbey-Gampert) Rivas-Martínez & al.
		public static string Parse(string input) {
			if (IsVirus(input)) {
				// return verbatim
			}
			return null;

		}

		public static string ExtractBitri(string input) {
			return null;
		}

		// translated from GNI Ruby -- def virus?(a_string)
		public static bool IsVirus(string name) {
			if (Regex.IsMatch(name, @"\sICTV\s*$")) // ICTV
				return true;

			if (Regex.IsMatch(name, @"\b(virus|viruses|phage|phages|viroid|viroids"
				    + @"|satellite|satellites|prion|prions)\b", 
				    RegexOptions.IgnoreCase))
				return true;

			//TODO: diacratics
			if (Regex.IsMatch(name, @"[A-Z]?[a-z]+virus\b"))
				return true;

			return false;
		}

		// def unknown_placement?(a_string)
		public static bool IsUnknownPlacement(string name) {
			//Anguilliformes incertae sedis
			//Anoplostoma dubium (sp incertae sedis)
			//Bacillales Family X. Incertae Sedis
			//Bacillariophyta incertae sedis
			//Desmodora papillata (sp incertae sedis)
			//Desmolaimus bibulbosus (sp inq incertae sedis)
			//INCERTAE SEDIS bellum Pease 1860 sec. DeFelice & Eldredge 2004
			//Incertae Sedis [Baleen]
			//Incertae Sedis [Toothed]
			//Incertae Sedis{57. 1}
			//Janiroidea incertae sedis
			//Phanaeus antiquus incertae sedis
			//Phanaeus antiquus incertae sedis HORN, 1876
			//Typhlops ahsanuli inc. sed. - WALLACH 2000 (nomen emend.)
			//Theristus stichotricha (sp inq incertae sedis)
			// incertae sedis
			if (Regex.IsMatch(name, @"incertae\s+sedis", RegexOptions.IgnoreCase))
				return true;

			// inc. sed. 
			if (Regex.IsMatch(name, @"inc\.\s*sed\.\b", RegexOptions.IgnoreCase))
				return true;

			// inc sed (6 more caught).. would be 7 without he \b.. should check what that one is?
			if (Regex.IsMatch(name, @"\binc[\.]?\s*sed[\.]?\b", RegexOptions.IgnoreCase))
				return true;

			return false;
		}

		public string CleanParse() {

		}


	}


}

