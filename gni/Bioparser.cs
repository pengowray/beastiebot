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

		public static bool isNontaxon(string name) {
			//2079874,"Un-named"
			//5636049,"Un-named clade"
			//2931414,"Un-named order"
			//21783386,"???"
			//21783382,"????"
			//21783372,"?????"
			//21783371,"??????"
			//22008109,"???????"
			//22008110,"????????"
			//10654149,"Ph-unknown" // phylum: unknown
			//11800334,"Zzunknown zzunknown"
			//11651300,"Zzz-todelete"
			//11651458,"Zzztrash"
			//11696085,"Zzztrashfamily"
			//11651282,"Zzzz-delete"
			//11651461,"Zzzztodelete"
			return true;
		}

		// translated from GNI Ruby -- def virus?(a_string)
		public static bool IsVirus(string name) {
			if (Regex.IsMatch(name, @"\sICTV\s*$")) // ICTV
				return true;

			// retroviruses (Retro-transcribing viruses > Retroviridae)

			//5277786,"RT-dsDNA"
			//5277787,"RT-ssRNA"
			//21789671,"RV-African clawed toad"
			//21789646,"RV-Bower bird"
			//21789677,"RV-Palmate newtI"
			//21789678,"RV-Palmate newtII"

			//other 
			//21785236,"-viridae"

			// ??
			//10653980,"Ph-cnidaria"
			//10654685,"Ph-porifera" // PHYLUM PORIFERA ?
			//10654149,"Ph-unknown"

			// not viruses
			//21792947,"CFP Integration vector pSMUC2+" (eukaryotic vector)
			//21793930,"cfp marker plasmid pWM1009" (Synthetic double-stranded DNA ring)

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
			// incertae sedis
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
			//Leptiotidae inc. sedis

			//dumb, nonstandard
			//13849786,"Incertae_sedis"
			//2814265,"Incerta_sedis"
			//18142977,"Incerlae sedis"
			//2405247,"Incerta SEDIS"
			//6285,"Spirobolida incerta sedis"
			//6313,"Symphyla_incerta_sedis"

			//not inc. sed.:
			//1382998,"Incurvaria sedella Busck 1915"

			//Zz*
			//11800305,"Zzrodent"
			//10110031,"Zzwallaby"
			//11800329,"Zzunknown finch-bill sp."
			//11800320,"Zzunknown large bird sp."
			//11800314,"Zzunknown passeriform sp."
			//11651460,"Zz-to"
			//last real:
			//292498,"Zyzzyzus warreni Calder, 1988"


			//5467607,"Aleocharinaeincertaesedis"
			if (Regex.IsMatch(name, @"incertae\s*sedis", RegexOptions.IgnoreCase))
				return true;

			// inc. sed., (expanded to catch "inc sed", "inc. sedis", "incerta sedis")
			// 17931156,"incert. sed."
			//12028579,"Liophis inserta sedis"
			if (Regex.IsMatch(name, @"\bin[cs](ert|erta)?[\.]?\s+sed(\.|is|\b)", RegexOptions.IgnoreCase))
				return true;

			if (name.StartsWith("Zz"))
				return true;

			return false;
		}

		public string CleanParse() {
			return null;
		}


	}


}

