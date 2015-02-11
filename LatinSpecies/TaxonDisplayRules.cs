using System;
using System.IO;

// TODO
// (done) lemurs, marsupials (in rules)
// display of trinoms + stocks/pops
// statistics: numbers: assessed, CR, threatened, DD, (recently) extinct
// statistics: are all from < 3 countries?
// add "low priority" option to '=' rules, to use the Wiki's names when they become available.
// rename '=' to 'common' or 'is-named' or 'in-english' or something

//Limnodynastidae redirects to Myobatrachidae, but both families are used by IUCN
using System.Collections.Generic;

namespace beastie {
	public class TaxonDisplayRules
	{

		//locations?, e.g. of Pitheciidae

		//Carnivora = carnivoran // meh

		// grammar:
		// (todo) X wikilink Y // don't wikilink to X, use Y instead (for disambiguation), e.g. Anura wikilink Anura (frog)
		// X = Y    -- use Y as the common name for (singular)
		// X = Y family  -- use "family" in the heading instead of "species" (for "cavy family")
		// X = Y species Z -- don't add "species" to name if already in name (eg Hylidae)
		// X includes Y. Have a blurb under the heading saying "Includes y"
		// (todo) X comprises Y. ditto.
		// // comment
		// X force-split // split taxa into lower ranks if available, even if there are few of them
		// X below Y Z // Place new category Y below existing category X, and make it rank Z

		public const string GeneralRules = @"
Cetartiodactyla includes dolphins, whales and even-toed ungulates
Cetartiodactyla force-split // Tylopoda, Artiofabula (so includes Baiji and Pygmy hog)
Mormoopidae includes ghost-faced bats, moustached bats, and naked-backed bats // includes? or complete list? or are they synonyms?
Mystacinidae = New Zealand short-tailed bat // only one genus, two species
Natalida = funnel-eared bat
Pteralopex flanneryi = greater monkey-faced bat // this entry only needed temporarily (redirect was missing)
Rhinolophus hilli = Hill's horseshoe bat
Dasyuromorphia includes most of the Australian carnivorous marsupials
// Diprotodontia include the kangaroos, wallabies, possums, koala, wombats, and many others. // too long. (all marsupial mammals). 
// Eulipotyphla - too long, obvious from species list
Dendrolagus mayri = Wondiwoi tree-kangaroo // sometimes considered a subsp.
Congosorex phillipsorum = Phillips' Congo shrew
// Lagomorpha includes hares, rabbits, pikas // obvious from list
Atelidae = atelid
Cheirogaleidae = cheirogaleid
Hominidae = great ape // note: Hominid refers to humans and relatives of humans closer than chimpanzees
Caviidae = cavy family // just 'cavy' is ambiguous
// Cricetidae includes true hamsters, voles, lemmings, and New World rats and mice
// Habromys schmidlyi = Schmidly's Deer Mouse // but shares a name with Peromyscus schmidlyi ??
Heteromyidae = heteromyid
//Echimyidae = echimyid
Echimyidae = spiny rat // and their fossil relatives
Muridae = murid
Muridae includes true mice and rats, gerbils, and relatives
Bovidae = bovid
Bovidae includes cloven-hoofed, ruminant mammals
// Muridae force-split // maybe.. not really needed

// lemurs
Archaeolemuridae below Lemuroidea Superfamily //extinct
Cheirogaleidae below Lemuroidea Superfamily 
Daubentoniidae below Lemuroidea Superfamily 
Indriidae below Lemuroidea Superfamily 
Lemuridae below Lemuroidea Superfamily 
Lepilemuridae below Lemuroidea Superfamily // Sportive lemur
Megaladapidae below Lemuroidea Superfamily // extinct
Palaeopropithecidae below Lemuroidea Superfamily //extinct
Lemuroidea = lemur

// marsupials
Didelphimorphia below Marsupialia Infraclass
Paucituberculata below Marsupialia Infraclass
Microbiotheria below Marsupialia Infraclass
Yalkaparidontia below Marsupialia Infraclass // extinct
Dasyuromorphia below Marsupialia Infraclass // quolls, thylacines, devils, dunnarts, antechinuses
Peramelemorphia below Marsupialia Infraclass 
Notoryctemorphia below Marsupialia Infraclass
Diprotodontia below Marsupialia Infraclass // koalas etc

// other:
Peripatopsis leonina = Lion's Hill velvet worm
Telemidae = six-eyed spider

// Frogs
Anura = frog
Anura wikilink Anura (frog) // avoid disambig page, though could just link to Frog
Aromobatidae = cryptic forest frog
Arthroleptidae = screeching frog // also called squeakers
// Brevicipitidae = rain frog // rain frog is ambiguous
Craugastoridae = fleshbelly frog
Cycloramphidae == Cycloramphinae // spelled slightly differently on wikipedia (but isn't a redirect to common name)
Dicroglossidae = fork-tongued frog
// Eleutherodactylidae = rain frog // rain frog is ambiguous
Hylidae includes tree frog species and their allies
Hyperoliidae = African reed frog // has several other common names http://research.amnh.org/vz/herpetology/amphibia/Amphibia/Anura/Hyperoliidae
Leiopelmatidae = New Zealand primitive frog // or New Zealand frog // http://research.amnh.org/vz/herpetology/amphibia/Amphibia/Anura/Leiopelmatidae
Leptodactylidae = southern frog
Limnodynastidae = Australian ground frog // wikipedia common name confirmed http://research.amnh.org/vz/herpetology/amphibia/Amphibia/Anura/Limnodynastidae
// Limnodynastidae redirects to Myobatrachidae, where it is called Limnodynastinae, and is a subfamily // not sure about this redirect TODO:
// Mantellidae = ... English Names: None noted
Megophryidae = litter frog // also megophryid
Micrixalidae = dancing frog // also  tropical frogs, and torrent frogs (Micrixalus is monotypic within Micrixalidae)
//Micrixalidae == Micrixalus
Microhylidae = narrow-mouthed frog
Myobatrachidae = Australian water frog // note: called Australian ground frogs on wiki
Nyctibatrachidae = robust frog
Myobatrachidae = Myobatrachidae// Myobatrachinae
// Odontophrynidae = English Names: None noted
Petropedetidae = African torrent frog
// Phrynobatrachidae == Phrynobatrachus (monophyletic)
Phrynobatrachidae = puddle frog
Pipidae = tongueless frog
// Pyxicephalidae = English Names: None noted
Ranixalidae = leaping frog 
// Ranixalidae == Indirana (genus) (monophyletic )
Rhacophoridae = shrub frog
Sooglossidae = Seychelles frog
Telmatobiidae = water frog // == Telmatobius (genus) ?
";

		string rules;

		// compiled:
		public Dictionary<string, string> taxonCommonName = new Dictionary<string, string>();
		public HashSet<string> forceSplit = new HashSet<string>();
		public Dictionary<string, string> below = new Dictionary<string, string>();
		public Dictionary<string, string> includes = new Dictionary<string, string>();

		public TaxonDisplayRules() {
			rules = GeneralRules;
		}

		public void Compile() {
			int lineNumber = 0;
			var reader = new StringReader(rules);
			string line;
			while ((line = reader.ReadLine()) != null) {
				lineNumber++;

				if (string.IsNullOrWhiteSpace(line))
					continue;

				// remove comments
				if (line.Contains("//")) {
					line = line.Substring(0, line.IndexOf("//"));
				}

				line = line.Trim();

				if (string.IsNullOrEmpty(line))
					continue;

				if (line.Contains(" = ")) {
					string[] parts = SplitAndAddToDictionary(line, " = ", lineNumber, taxonCommonName);

					// warn if -s ending
					if (parts != null) {
						string common = parts[0];
						if (common.EndsWith("s") && !common.Contains("species")) {
							Warning(lineNumber, line, "Common name appears to be plural (ends with 's'): " + common);
						}
					}

				} else if (line.Contains("force-split")) {
					string split = line.Substring(0, line.IndexOf("force-split"));
					split = split.Trim();
					if (string.IsNullOrEmpty(split)) {
						Error(lineNumber, line, "'force-split' missing argument.");
					}
					forceSplit.Add(split);

				} else if (line.Contains(" below ")) {
					SplitAndAddToDictionary(line, " below ", lineNumber, below);

				} else if (line.Contains(" includes ")) {
					SplitAndAddToDictionary(line, " includes ", lineNumber, includes);
				}
			}
		}

		string[] SplitAndAddToDictionary(string line, string seperator, int lineNumber, Dictionary<string,string> addToDictionary = null) {
			var parts = line.Split(new string[]{ seperator }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) {
				Error(lineNumber, line, 
					string.Format("'{0}' statement missing arguments. Needs something on either side: {1}", seperator, line));
				return null;
			} else {
				parts[0] = parts[0].Trim();
				parts[1] = parts[1].Trim();
				if (addToDictionary != null) 
					addToDictionary[parts[0]] = parts[1];

				return parts;
			}
		}

		void Warning(int lineNumber, string line, string warning) {
			Error(lineNumber, line, warning);
		}

		void Error(int lineNumber, string line, string error) {
			Console.Error.WriteLine("Error on line {0}: {1}", lineNumber, error);
			Console.Error.WriteLine("Line {0}: {1}", lineNumber, line);
		}
	}
}

