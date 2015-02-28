using System;
using System.Text.RegularExpressions;
using RestSharp.Contrib;

namespace beastie {
	public class GNIStrings
	{
		public GNIStrings() {
		}

		//public string RepairForMergingIntoNewIndex(string value) {
		//	return RepairNewlines(RepairEncoding(value));
		//}

		public static string RepairForUsage(string value) {
			return RepairCharChoices(value);
		}

		public static string EscapeNewlines(string value) {
			value = value.Replace("\n", @"\n");
			value = value.Replace("\r", "");
			value = value.Replace("\t", " ");
			return value;
		}

		// valid lowercase start: (de, di)
		// 22379324: Anhammus dalenii borneensis deJong, 1942
		// 8470031: Chenopodium superalbum F. DvoYák
		// 2289527: Cicindela asperula DuFabricius, 1821
		// 5681630: Bacterium thalassium (ZoBell and Upham) Krasil’nikov 1949

		// very sus
		// 2276835: Dosinia derupta RïøΩmer, 1860   (fixed)
		// 2375587: Schoettella celiae Fernandes & de MendonÁa 1998     (fixed)
		// 5303470: Amaryllidaceae JaumeSt.Hil.
		// 2279961: Brachydesmus (Brachydesmus) calcarius KovaÄçeviÄá, 1928; emend. Mrsic, 1988

		// of note: (normal sus)
		// 10418827: Agelas oroides (R³tzler & SarÓ 1962)
		// 12038754: Anthelephila consul (La FertÚ-SÚnectÞre, 1849)
		// 13312921: Aphanostephus skirrobasis Trel. ex Coville etBranner
		// 18238482: Aphidius kOnoi
		// 19086414: Apocheiridium leopoldi Vitali-diCastri 1962
		// 10862248: Aponogeton madagascariensis (Mirb.) H.W.E.vanBruggen
		// 13508261: Archizausodes biarticulatus (ItôTat, 1979)
		// 5607233: Argyra californica VanDuzee
		// 10462229: Artemisia cina O.C.BergrtC.F.Schmidt-Zitwer
		// 2551237: Arthroderma avaricum (Szathmáry) Ktivanec{?}, Janecková & Ot6enjLgek{?} 1977
		// 1395875: Audouinella spongicola (Weber-vanBosse) Stegenga
		// 17080986: Blefuscuiana speetonensis tunisiensis BouDagher-Fadel 1996
		// 16958596: Calisto anegadensis Smith, Miller & KcKenzie 1991
		// 18114202: Campodea (C.) oredonensis CondE 1951
		// 1797511: Camillina shaba FitzPatrick, 2005
		// 17923031: CalymenidaeEndocrania
		// 17923823: CalymenidaePharostoma
		// 10042979: Ceratophrys ornata [AuthorYear]
		// 18943298: Cerceris anUmim
		// 18089837: Chrysobatrachus cupreonitens ManaYas 1949
		// 10748251: Corbula mediterranea DaCosta
		// 12039119: Copelatus lineatus GuÚrin-MÚneville, 1838
		// 2877990: Coussapoa asperifolia Trecul -- See TrTcul
		// 11346204: Cupixi virus {S segment: [AF512832] Cupixi virus - BeAn 119303 ICTV
		// 2553698: Dekkeromyces phaseolosporus (Shehata et al. {?}) Santa MarÃa & Sánchez Nieto
		// 2076139: Anaerobaculum Rees et al. 1997 emend. Menes and MuxÌ 2002

		// repairable errors:
		// 13423845: Arum maculatum L. f. flavescens (Melzer exJanchen) Riedl
		// 17849712: Atysa uetsukii Ch&#x000FB;j&#x000F4;
		// 5659370: Bacterium milletiae (Kawakami andYoshida) Burgvits 1935
		// 2405192: Canthomoechus Pereira and MartÌnez, 1959
		//TODO: check for apostrophe as 2nd char
		// todo: two different functions for very sus (weird camel) and regular sus (camel) and for symbols (nyi)
		public static bool IsCharacterChoiceSuspicious(string value, bool verySusOnly=true) {
			// check for capitals in the middle of a word
			var matches = Regex.Matches(value, @"\b(.+?)\b"); // find words
			foreach (var match in matches) {
				if (match.ToString().StartsWith("Mc") || match.ToString().StartsWith("Mac"))
					continue;

				if (match.ToString().StartsWith("Fitz"))
					continue;

				if (match.ToString().StartsWith("La") || match.ToString().StartsWith("Le"))
					continue;
					
				if (match.ToString().StartsWith("De") || match.ToString().StartsWith("de"))
					continue;

				if (match.ToString().StartsWith("Di") || match.ToString().StartsWith("di"))
					continue;

				if (match.ToString().StartsWith("Du"))
					continue;
				
				if (match.ToString() == "ZoBell")
					continue;

				bool prevLower = false;
				foreach (char c in match.ToString()) {
					if (prevLower) {
						if (char.IsUpper(c)) {
							if (verySusOnly) {
								if (c > 'Z') {
									return true;
								}
							} else {
								return true;
							}
						}
					}

					if (char.IsLower(c)) {
						prevLower = true;
					}
				}
			}
			return false;
		}

		public static string Repair(string name) {
			name = RepairEncoding(name);
			name = RepairHTMLEncoding(name);
			name = RepairCharChoices(name);
			name = RepairWhitespace(name); // note: will also turn \n into a space, i think
			name = name.Normalize();  // Unicode normalization (Form-C) https://msdn.microsoft.com/en-us/library/system.text.normalizationform(v=vs.110).aspx
			return name;
		}


		public static string ExtractTaxon() {
			//see also: https://github.com/GlobalNamesArchitecture/biodiversity
			// https://github.com/GlobalNamesArchitecture/biodiversity/blob/master/lib/biodiversity/parser.rb

			// Iberis spathulata J.P.Bergeret subsp. lereschiana (Barbey-Gampert) Rivas-Martínez & al.

			return null;
		}

		/**
		 * repair the encoding, and repair the char choices only if the encoding is bad.
		 */
		public static string RepairEncodingMaybeMore(string value) {
			string fixedName = RepairEncoding(value);
			if (value != fixedName) {
				return RepairCharChoices(fixedName);
			}

			return value;
		}

		public static string RepairHTMLEncoding(string name) {
			name = HttpUtility.HtmlDecode(name);
			return name;
		}

		public static string RepairEncoding(string value) {
			string fixedName = value.FixUTFv2();

			if (value != fixedName) {
				fixedName = fixedName.Replace("�*vila Ortíz", "Ávila Ortíz"); // nothing will fix Ã*vila OrtÃ­z
				fixedName = fixedName.Replace("�*lvarez", "Álvarez"); // Ã*lvarez & GonzÃ¡lez
					fixedName = fixedName.Replace(" � ", " × "); // "Genista × rivasgodayana AndrÃ©s & Llamas" => "Genista � rivasgodayana Andrés & Llamas"
				fixedName = fixedName.Replace("“bergeri�*", "'bergeri'"); // Homopus â€œbergeriâ€* .. probably was meant to be: “bergeri“ (smart quotes)
				fixedName = fixedName.Replace("Bell�*", "Bellù"); // BellÃ*

				//Existing name:  Myriogramme okhaã«nsis BÃ¸rgesen
				//Suggested name: Myriogramme okha�nsis Børgesen
				fixedName = fixedName.Replace("okha�nsis", "okhaånsis");

				//Existing name:  Xanthidium antilopaeum (BrÃ©bisson) Kützing
				//Suggested name: Xanthidium antilopaeum (Brébisson) K�tzing
				//Existing name:  Nitzschia longissima (BrÃ©bisson in Kützing) Ralfs
				//Suggested name: Nitzschia longissima (Brébisson in K�tzing) Ralfs
				fixedName = fixedName.Replace("K�tzing", "Kützing");

				//Existing name:  Packera bellidifolia (Kunth) W.A. Weber & A. LÃœve
				//Suggested name: Packera bellidifolia (Kunth) W.A. Weber & A. LÜve
				fixedName = fixedName.Replace("LÜve", "Löve");

				//(note: species!)
				//Existing name:  Polyedrium tetraã«dricum NÃ¤geli
				//Suggested name: Polyedrium tetra�dricum Nägeli 
				fixedName = fixedName.Replace("tetra�dricum", "tetraëdricum"); // "tetra\u0091dricum" == tetra‘dricum (win) tetraëdricum (mac roman)

				//(note taxon! though not really a species)
				//Existing name:  Anomoeoneis sphaerophora var. gã¼ntheri O. MÃ¼ller
				//Suggested name: Anomoeoneis sphaerophora var. g�ntheri O. Müller
				fixedName = fixedName.Replace("g�ntheri", "güntheri"); // Ã¼ = ü. ã¼ = lowercased Ã¼

				//Existing name:  Barbus anniae LevÃñque, 1983
				//Suggested name: Barbus anniae Lev̖que, 1983
				fixedName = fixedName.Replace("Lev̖que", "Lévêque"); // via LevÃñque

				//Existing name:  Monoraphidium minutum (Nägeli) KomÃ¡rkovÃ¡-legnerovÃ¡
				//Suggested name: Monoraphidium minutum (N�geli) Komárková-legnerová
				fixedName = fixedName.Replace("N�geli", "Nägeli"); // // via LevÃñque

				//Existing name:  Spiniferomonas involuta (B. Å…. Jacobsen) P. A. Siver (better).. fix elsewhere
				//Suggested name: Spiniferomonas involuta (B. Ņ. Jacobsen) P. A. Siver (wrong)
				if (value.Contains("Å…."))
					return value;  // actually, don't attempt to fix encoding.. fix with RepairCharChoicesName()

				return fixedName;
			}

			return value;
		}

		public static string RepairCharChoices(string name) {

			// after fixing bad encodings
			name = name.Replace("RamÆrez", "Ramírez"); // no encoding will fix "RamÃ†rez"
			name = name.Replace("RuÆz", "Ruíz"); // no encoding will fix "RuÃ†z"
			name = name.Replace("ManrÆquez", "Manríquez"); // ManrÃ†quez
			name = name.Replace("GarcÆa", "García"); // nothing will fix "GarcÃ†a"

			name = name.Replace("AntonÌn", "Antonín"); // AntonÃŒn
			name = name.Replace("MartÌn", "Martín"); // MartÃŒn

			name = name.Replace("Mƒll", "Müll"); // no encoding will fix "MÆ’ll".
			name = name.Replace("DugËs", "Dugès"); // no encoding will fix "DugÃ‹s"
			name = name.Replace("MoÎnne", "Moënne"); // no encoding will fix "MoÃŽnne"
			name = name.Replace("HollÛs", "Hollós"); // nothing will fix "HollÃ›s"

			name = name.Replace("WichanskÞ", "Wichanský");

			//Existing name:  Paludestrina thermalis BoubÃ‰Ã‰
			//Suggested name: Paludestrina thermalis BoubÉÉ
			name = name.Replace("BoubÉÉ", "Boubée"); // via BoubÃ‰Ã‰ (Nérée Boubée)

			//Existing name:  Stuckenia pectinata (L.) BÃœrner
			name = name.Replace("BÜrner", "Böerner"); // via "BÃœrner", aka "Borner"

			//Existing name:  Zizaniopsis miliacea (Michx.) DÃœll & Asch.
			//Suggested name: Zizaniopsis miliacea (Michx.) DÜll & Asch.
			name = name.Replace("DÜll", "Döll"); // via "DÃœll"

			//Existing name:  Caranx LacepÃ‹de, 1801
			//Suggested name: Caranx LacepËde, 1801
			name = name.Replace("LacepËde", "Lacépède"); // via "LacepÃ‹de"

			// other (not picked up as wrongly encoded)

			name = name.Replace("WichanskÃ", "Wichanský");
			name = name.Replace("BÃrner", "Böerner");
			name = name.Replace("Ch&#x000FB;j&#x000F4;", "Chûjô"); //TODO: generalize
			name = name.Replace("MarÃa", "María");
			name = name.Replace("RïøΩmer", "Römer");
			//2375587: Schoettella celiae Fernandes & de MendonÁa 1998
			name = name.Replace("MendonÁa", "Mendonça");

			name = name.Replace('ſ', 's'); // old 's'

			// punctuation

			//Existing name:  Spiniferomonas involuta (B. Å…. Jacobsen) P. A. Siver (ok kinda.. several hits without the "...")
			//Suggested name: Spiniferomonas involuta (B. Ņ. Jacobsen) P. A. Siver (wrong.. one hit)
			name = name.Replace("….", "."); // remove "..."

			// 11390612: Aglaia rubiginosa (Hiern\u007F) Pannell
			// 10573352: Pleurotus ostreatus cv. Florida, (Jacquin\u007F\u007F : Fries) Kummer
			name = name.Replace("\u007F", "");

			//weirdness
			//22465622: Arrhenatherum tuberosum ssp. baeticum (Romero\\r\\nZarco) Rivas Mart. , Fern. Gonz. & Sánchez Mata
			//22441384: Lithospermum arvense ssp. sibthorpianum\\r\\nLithospermum arvense L. ssp. sibthorpianum (Griseb.) Stoj (Griseb.) Stoj. & Stef.
			//22427736: Trichiurus japonicus Temminck & Schlegel, 1844\\r\\nTemminck & Schlegel 1844
			name = name.Replace(@"\\r\\n", " ");

			if (name.EndsWith(" 0")) {
				name = name.Substring(0, name.Length - 2);
			}


			return name;
		}

		public static string RepairWhitespace(string value) {
			//TODO: (maybe elsewhere)
			//19024652,"Abax pyrenaeus (Dej. )"
			//17352063,"Aades franklini (Heller )"

			//TODO: check for matching brackets

			// tabs: // fixed by NormalizeSpaces() below
			// 22063467 as C# literal: Lasionycta perplexella\tCrabo et Lafontaine, 2009
			// 12054467 as C# literal: Desmanthus illinoiensis\tillinoiensis (Michx.) MacMillan ex Rob. & Fern.
			//value = value.Replace("\t", " "); 

			// Stuckenia ×suecica (K. Richt.) Kartesz [filiformis × pectinata]
			value = value.Replace("×suecica", "× suecica"); // TODO: generalize

			//TODO:
			//Myristica cornutifolia J. Sinclair subsp. elegans W. J. de Wilde 
			//Myristica cornutiflora J. Sinclair subsp. elegans W. J.de Wilde


			return value.NormalizeSpaces();
		}

		public static string SeverNewlines(string value) {
			// keeps only the first line
			// for what is on the other lines, see: bad-records-newlines.txt

			return value.Split("\n\r".ToCharArray(), 2)[0];
		}

	}
}

