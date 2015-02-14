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

// mammal groups/ranks
Cetartiodactyla includes dolphins, whales and even-toed ungulates
Cetartiodactyla force-split // Tylopoda, Artiofabula (so includes Baiji and Pygmy hog)
Dasyuromorphia includes most of the Australian carnivorous marsupials
Diprotodontia include the kangaroos, wallabies, possums, koala, wombats, and many others. // too long. (all marsupial mammals). 
// Eulipotyphla - too long, obvious from species list
Dendrolagus mayri = Wondiwoi tree-kangaroo // sometimes considered a subsp.
Congosorex phillipsorum = Phillips' Congo shrew
Lagomorpha includes hares, rabbits, pikas // obvious from list
Atelidae = atelid
Cheirogaleidae = cheirogaleid
Hominidae = great ape // note: Hominid refers to humans and relatives of humans closer than chimpanzees
Caviidae = cavy family // just 'cavy' is ambiguous
Cricetidae includes true hamsters, voles, lemmings, and New World rats and mice
// Habromys schmidlyi = Schmidly's Deer Mouse // but shares a name with Peromyscus schmidlyi ??
Heteromyidae = heteromyid
//Echimyidae = echimyid
Echimyidae = spiny rat // and their fossil relatives
Muridae = murid
Muridae includes true mice and rats, gerbils, and relatives
Bovidae = bovid
Bovidae includes cloven-hoofed, ruminant mammals
// Muridae force-split // maybe.. not really needed

// mammal species
Addax nasomaculatus = addax // monotypic genus is common name
Hippopotamus amphibius = hippopotamus // monotypic genus is common name
Caracal caracal = caracal  // monotypic genus is common name (also called desert lynx)
Soriculus nigrescens = Himalayan shrew // Soriculus is monotypic 
Indri indri = indri // monotypic genus is common name (also called babakoto)
Santamartamys rufodorsalis = red-crested tree rat // Santamartamys is monotypic, but isn't the common name
Lycaon pictus = African Wild Dog // temporary (new redirect)
Monodelphis unistriatus = One-striped opossum  // temporary (new/missing redirect)
Neomonachus schauinslandi = Hawaiian monk seal  // temporary (new/missing redirect)
Monachus schauinslandi = Caribbean monk seal // temporary  (new/missing redirect) // aka West Indian seal, sea wolf

// lemurs
Archaeolemuridae below Lemuroidea superfamily //extinct
Cheirogaleidae below Lemuroidea superfamily
Daubentoniidae below Lemuroidea superfamily 
Indriidae below Lemuroidea superfamily
Lemuridae below Lemuroidea superfamily 
Lepilemuridae below Lemuroidea superfamily // Sportive lemur
Megaladapidae below Lemuroidea superfamily // extinct
Palaeopropithecidae below Lemuroidea superfamily //extinct
Lemuroidea = lemur

// marsupials
Didelphimorphia below Marsupialia infraclass
Paucituberculata below Marsupialia infraclass
Microbiotheria below Marsupialia infraclass
Yalkaparidontia below Marsupialia infraclass // extinct
Dasyuromorphia below Marsupialia infraclass // quolls, thylacines, devils, dunnarts, antechinuses
Peramelemorphia below Marsupialia infraclass 
Notoryctemorphia below Marsupialia infraclass
Diprotodontia below Marsupialia infraclass // koalas etc

// New World monkeys
Callitrichidae below Platyrrhini parvorder // marmosets and tamarins
Cebidae below Platyrrhini parvorder  // capuchins and squirrel monkeys
Aotidae below Platyrrhini parvorder // night or owl monkeys (douroucoulis)
Pitheciidae below Platyrrhini parvorder // titis, sakis and uakaris
Atelidae below Platyrrhini parvorder  // howler, spider, woolly spider and woolly monkeys

// Lorisoidea
Lorisidae below Lorisoidea superfamily // lorises, pottos, and angwantibos
Galagidae below Lorisoidea superfamily // galagos

// Fish group
Agnatha below Fish paraphyletic-group
Actinopterygii below Fish paraphyletic-group
Cephalaspidomorphi below Fish paraphyletic-group // lampreys and fossil species (disputed, but used by IUCN for lampreys)
Chondrichthyes below Fish paraphyletic-group
Placodermi below Fish paraphyletic-group
Sarcopterygii below Fish paraphyletic-group


// Fish classes
Agnatha = jawless fishes
Actinopterygii = ray-finned fishes
Chondrichthyes = cartilaginous fishes
Placodermi = armoured fishes // fossil
Sarcopterygii = lobe-finned fishes

// Frog groups
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

// misc/other (todo more) .. random ones
Haematopinus oliveri = pygmy hog-sucking louse
Peripatopsis leonina = Lion's Hill velvet worm
Telemidae = six-eyed spider

// CETACEA
// IUCN has all Cetecea under CETARTIODACTYLA (order on iucn, unranked on wiki)
// so group them together as unranked (it's an order on wiki)
//TODO: remove ranks which are subordinate to each other (remove the one not used by IUCN)
Balaenidae below Cetacea unranked // Right whales and bowheadwhale
Balaenopteridae below Cetacea unranked // Rorquals
Eschrichtiidae below Cetacea unranked 
Cetotheriidae below Cetacea unranked 
Delphinidae below Cetacea unranked  // Dolphin
Monodontidae below Cetacea unranked 
Phocoenidae below Cetacea unranked  // Porpoises
Physeteridae below Cetacea unranked  // Sperm whalefamily
Kogiidae below Cetacea unranked  // – MSW3 treats Kogia as a member ofPhyseteridae
Iniidae below Cetacea unranked
Lipotidae below Cetacea unranked // – MSW3 treats Lipotes as a member of Iniidae
Pontoporiidae below Cetacea unranked // – MSW3 treats Pontoporia as a member of Iniidae
Platanistidae below Cetacea unranked
Ziphidae below Cetacea unranked

// CETACEA: extinct and other cetecea from wikispecies
Aetiocetidae below Cetacea unranked
Eomysticetidae below Cetacea unranked
Ambulocetidae below Cetacea unranked
Basilosauridae below Cetacea unranked
Pakicetidae below Cetacea unranked
Protocetidae below Cetacea unranked
Remingtonicetidae below Cetacea unranked
// Balaenidae below Cetacea unranked // double
// Balaenopteridae below Cetacea unranked // double
// Eschrichtiidae below Cetacea unranked // double
Neobalaenidae below Cetacea unranked
// Cetotheriidae below Cetacea unranked // double
Janjucetidae below Cetacea unranked

//Chiroptera
Nycteridae = slit-faced bat
Hipposideridae = Old World leaf-nosed bat
Natalidae = funnel-eared bat
Mormoopidae includes ghost-faced bats, moustached bats, and naked-backed bats // includes? or complete list? or are they synonyms?
Mystacinidae = New Zealand short-tailed bat // only one genus, two species
Natalida = funnel-eared bat
Pteralopex flanneryi = greater monkey-faced bat // this entry only needed temporarily (redirect was missing)
Rhinolophus hilli = Hill's horseshoe bat
Rhinopomatidae = mouse-tailed bat
Thyropteridae = Disc-winged bat
Megadermatidae = false vampire bat

//microbats
Craseonycteridae below Microchiroptera suborder
Emballonuridae below Microchiroptera suborder
Furipteridae below Microchiroptera suborder // just two bat species
Hipposideridae below Microchiroptera suborder // Old World leaf-nosed bat species
Megadermatidae below Microchiroptera suborder
Molossidae below Microchiroptera suborder
Mormoopidae below Microchiroptera suborder
Mystacinidae below Microchiroptera suborder
Myzopodidae below Microchiroptera suborder
Natalidae below Microchiroptera suborder
Noctilionidae below Microchiroptera suborder
Nycteridae below Microchiroptera suborder
Phyllostomidae below Microchiroptera suborder
Rhinolophidae below Microchiroptera suborder
Rhinopomatidae below Microchiroptera suborder
Thyropteridae below Microchiroptera suborder
Vespertilionidae below Microchiroptera suborder

//bat species
Nyctophilus nebulosus = New Caledonian long-eared bat // temporary (new redirect)
Eumops floridanus = Florida bonneted bat
// Pteronotus paraguanensis = ??? no common name

//rodent groups
Abrocomidae below Hystricomorpha suborder
Anomaluridae below Anomaluromorpha suborder
Aplodontiidae below Sciuromorpha suborder
Bathyergidae below Hystricomorpha suborder
Capromyidae below Hystricomorpha suborder
Castoridae below Castorimorpha suborder
Caviidae below Hystricomorpha suborder
Chinchillidae below Hystricomorpha suborder
Cricetidae below Myomorpha suborder
Ctenodactylidae below Hystricomorpha suborder
Ctenomyidae below Hystricomorpha suborder
Cuniculidae below Hystricomorpha suborder
Dasyproctidae below Hystricomorpha suborder
Diatomyidae below Hystricomorpha suborder
Dinomyidae below Hystricomorpha suborder
Dipodidae below Myomorpha suborder
Echimyidae below Hystricomorpha suborder
Erethizontidae below Hystricomorpha suborder
Geomyidae below Castorimorpha suborder  // Historically below Myomorpha
Gliridae below Sciuromorpha suborder // Historically below Myomorpha
Heteromyidae below Castorimorpha suborder // Historically below Myomorpha
Hystricidae below Hystricomorpha suborder
Muridae below Myomorpha suborder
Myocastoridae below Hystricomorpha suborder
Nesomyidae below Myomorpha suborder
Octodontidae below Hystricomorpha suborder
Pedetidae below Anomaluromorpha suborder
Petromuridae below Hystricomorpha suborder
Platacanthomyidae below Myomorpha suborder
Sciuridae below Sciuromorpha suborder
Spalacidae below Myomorpha suborder
Thryonomyidae below Hystricomorpha suborder

// rodent families not listed by iucn, but included anyway (though many more extinct families not included)
Parapedetidae below Anomaluromorpha suborder
Zegdoumyidae below Anomaluromorpha suborder // (extinct) 'sometimes placed in the Anomaluromorpha'
Eutypomyidae below Castorimorpha suborder // extinct
Rhizospalacidae below Castorimorpha suborder // extinct
Eomyidae below Castorimorpha suborder  // extinct
Calomyscidae below Myomorpha suborder

";

		string rules;

		// compiled:
		public Dictionary<string, string> taxonCommonName = new Dictionary<string, string>();
		public HashSet<string> forceSplit = new HashSet<string>();
		public Dictionary<string, string> below = new Dictionary<string, string>();
		public Dictionary<string, string> includes = new Dictionary<string, string>();
		public Dictionary<string, string> wikilink = new Dictionary<string, string>();

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
						string common = parts[1];
						if (common.EndsWith("s") && !common.Contains("species") && !common.EndsWith("fishes")) {
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

				} else if (line.Contains(" wikilink")) {
					SplitAndAddToDictionary(line, " wikilink ", lineNumber, wikilink);
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
				if (addToDictionary != null) {
					addToDictionary[parts[0]] = parts[1];
				}

				return parts;
			}
		}

		void Warning(int lineNumber, string line, string warning) {
			Console.Error.WriteLine("Warning on line {0}: {1}", lineNumber, warning);
			Console.Error.WriteLine("Line {0}: {1}", lineNumber, line);
		}

		void Error(int lineNumber, string line, string error) {
			Console.Error.WriteLine("Error on line {0}: {1}", lineNumber, error);
			Console.Error.WriteLine("Line {0}: {1}", lineNumber, line);
		}
	}
}

