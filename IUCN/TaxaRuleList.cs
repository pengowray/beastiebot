using System;
using System.IO;

// TODO
// (done) lemurs, marsupials (in rules)
// (done) display of trinoms + stocks/pops
// add "low priority" option to '=' rules, to use the Wiki's names when they become available.
// display of infraspecies rank ("var.", etc)
// statistics: numbers: assessed, CR, threatened, DD, (recently) extinct
// statistics: are all from < 3 countries?
// rename '=' to 'common' or 'is-named' or 'in-english' or something

// weird page with two taxoboxes: https://en.wikipedia.org/wiki/Bongo_(antelope)

//Limnodynastidae redirects to Myobatrachidae, but both families are used by IUCN
using System.Collections.Generic;

namespace beastie {
    
    public class TaxaRuleList {


        //locations?, e.g. of Pitheciidae

        //Carnivora = carnivoran // meh

        // grammar:
        // X wikilink Y // don't wikilink to X, use Y instead (for disambiguation), e.g. Anura wikilink Anura (frog)
        // X = Y    -- use Y as the common name for (singular)
        // X = Y family  -- use "family" in the heading instead of "species" (for "cavy family")
        // X = Y species Z -- don't add "species" to name if already in name (eg Hylidae)
        // X plural Y -- Y is the plural common name for X, but don't specify a singular common name
        // X includes Y. Have a blurb under the heading saying "Includes y"
        // X comprises Y. Have grey text in brackets under the heading with what it comprises.
        // // comment
        // X force-split // split taxa into lower ranks if available, even if there are few of them
        // X below Y Z // Place new category Y below existing category X, and make it rank Z

        // terminology i'm going to stick to:
        // evalated = includes DD
        // fully assessed = excludes DD

        //quotes from IUCN for language:

        // Nearly one-quarter (22 %) of the world's mammal species are known to be globally threatened or extinct, 63 % are known to not be threatened, and 15 % have insufficient data to determine their threat status.
        // There are 76 mammals considered to have gone Extinct since 1500, and two are Extinct in the Wild.
        // http://www.iucnredlist.org/initiatives/mammals/analysis
        // "threat status"

        // Of the 5,487 mammal species assessed, nearly one-quarter of species (22.2 %) are globally threatened or extinct, representing 1,219 species (Figure 1 and Table 1). Seventy-six of the 1,219 species are considered to be Extinct (EX), and two Extinct in the Wild (EW). Another 3,432 species are not considered to be threatened at present, being classified in the IUCN Red List Categories of Near Threatened (NT) or Least Concern (LC), while there was insufficient information available to assess the status of an additional 836 species (IUCN Red List Category Data Deficient).
        // The percentage of mammals assessed as Data Deficient (15%) in 2008 is higher than previously found for mammals on the IUCN Red List in 2004 (7.8%).
        // "insufficient information available to assess the status of "
        // "assesed as DD"

        // Because many Data Deficient species are likely to have small distributions or populations, or both, they are intrinsically likely to be threatened. 
        // -- http://www.iucnredlist.org/initiatives/mammals/description/limitations

        // DD =  ("inadequate information to make a[n] ... assessment")
        // DD: IUCN Red List Categories, Data Deficient: A taxon is Data Deficient when there is inadequate information to make a direct, or indirect, assessment of its risk of extinction based on its distribution and/or population status. A taxon in this category may be well studied, and its biology well known, but appropriate data on abundance and/or distribution are lacking. Data Deficient is therefore not a category of threat. Listing of taxa in this category indicates that more information is required and acknowledges the possibility that future research will show that threatened classification is appropriate. See 2001 IUCN Red List Categories and Criteria version 3.1.
        // -- http://www.iucnredlist.org/initiatives/mammals/description/glossary
        // not a "category of threat"

        // DD = assessed: "asssessed as DD"
        // "not all species groups have been fully evaluated, and also by the fact that some species have so little information available that they can only be assessed as Data Deficient (DD)"
        // // To account for the issues raised above, proportion of threatened species is only reported for the more completely evaluated groups 
        // -- http://www.iucnredlist.org/about/summary-statistics
        // "fully evaluated" "assessed as Data Deficient" "completely evaluated groups"

        // DD = "incompletely evaluated"
        // http://www.iucnredlist.org/about/summary-statistics

        // "non-Data Deficient species" 

        // DD = can't be assessed.
        // "It may be appropriate (especially for Data Deficient forms) to give them the same degree of attention as threatened taxa, at least until their status can be assessed."
        // -- http://www.iucnredlist.org/static/categories_criteria_3_1



        public const string GeneralRules = @"
Mammalia blurb The IUCN claims its Red List dataset contains information on the conservation status of all of the world's known mammal species.<ref>http://www.iucnredlist.org/initiatives/mammals/process/methods</ref> 

// top
Mammalia = mammal
Mammalia adj mammalian
Mollusca = mollusc ! molluscs // (or mollusk ! mollusks). Go with spelling which matches other wikipedia pages, e.g. Lists of molluscs by location
Arthropoda = arthropod ! arthropods
Chordata = chordate ! chordates
Caudata = salamander ! salamanders // also includes other extinct groups
Salamandridae comprises true salamanders and newts
Invertebrate = invertebrate ! invertebrates
Invertebrate adj invertebrate
Aves adj avian

// wikilinks
Anura = frog ! frogs
Anura wikilink Anura (frog) // avoid disambig page, though could just link to Frog
Hygrophila wikilink Hygrophila (gastropod)
//TODO: separate wikilinks for plant and animal kingdoms.
// note Hygrophila (plant) is a genus so it shouldn't be linked to in current lists
// likewise Anura (plant) is a genus, and also is a synonym of Arctium, so two reasons it wont be linked
// Possible new syntax if ambiguous names become more common:
//  Hygrophila wikilink Hygrophila (gastropod) : Animalia; Hygrophila (plant) : Plantae
//  Anura wikilink Anura (frog) : Animalia; Arctium : Plantae

// Wikipedia edits to absorb
Equus africanus = African wild donkey // Less suggestive common name, 104.207.218.182. https://en.wikipedia.org/w/index.php?title=List_of_critically_endangered_mammals&diff=716299629&oldid=712452632
Triaenops rufus wikilink Triaenops menamena // disambig by R'n'B. https://en.wikipedia.org/w/index.php?title=List_of_least_concern_mammals&diff=712365052&oldid=706061845

//iucn red list fixes, alterations
Procambarus lucifugus lucifugus = Withlocoochee light-fleeing cave crayfish // listed as 'Withlocoochee Light-fleeing Cave Crayfis'
Sphenomorphus decipiens = black-sided sphenomorphus // listed as 'Black-sided Sphenomorphuseng'
Cobitis puncticulata = brown spined loach // listed as 'Brown Sspined Loach'
Garra dunsirei = Tawi Atair garra // has a dot 'Tawi Atair Garra.' (Tawi Atair is a place)
Osteochilus microcephalus = Bonylip barb (preferred over 'Pla rong mai tub')
Pseudophoxinus elizavetae = Sultan Sazlığı minnow // listed as 'Sultan sazl?g? minnow'
Zamia vazquezii = little chamal // listed as 'Little chamal (náh.--chamalillo)' // aka Amigo del Maíz 
Cypripedium arietinum = ram's head lady slipper // listed as 'Spurred-lip cypripedium - ram's head lady slipper'
Haplochromis sauvagei = rock kribensis // listed as 'Rock Kribensis0'
Cheilopogon dorsomacula = backspot flying fish // listed as 'Backspot Flying Fish (fb)'
Wolterstorffina mirei = Mount Okou Wolterstorff toad // listed as 'Mout Okou Wolterstorff Toad'
Hypleurochilus aequipinnis = West African speckled blenny // listed as 'Oyster Blenny, West AfricanSspeckled Blenny'
Leptolebias minimus = minute pearlfish // listed as 'Annual Tropiacl Killifish, Minute Pearlfish'
Cobitis levantina = Orontes spined loach // listed as 'Orontes Sspined Loach'
Cobitis fahireae = Aegean spined loach // listed as 'Aegean Sspined Loach'
Gobio maeandricus = Işıklı gudgeon
Potamothrissa acutirostris = sharpnosed sawtooth pellonuline // listed as 'Sharpnosed sawtooth pellonul., Sharpnosed sawtooth pellonuline, Sharpnosed sawtooth pellonuline (FB)'
Potamotrygon falkneri = largespot river stingray // listed as 'Reticulated Freshwater Stingray., Largespot River Stingray'
Geothelphusa candidiensis = Sun Moon Lake Ze crab // listed as 'Sun Moon Lake Ze Crabs' (plural + has trouble with capitalization)
Delphinus delphis ponticus = Black Sea common dolphin // prevent it finding IUCN name 'Common dolphin' which is for the whole species
Pseudocheirus occidentalis = Western ringtail possum // prevent it choosing 'Western ringtail'
Alburnus attalus = Bakır Shemaya // (Bak?r Shemaya)
Chondrostoma meandrense = Işıklı nase  // (Is?kl? Nase)
Gobio maeandricus = Işıklı gudgeon // (Is?kl? Gudgeon)
Hainania serrata = Hainan minnow // , 海南 (???): Common name contains question mark
Metzia formosae = Taiwan lesser-bream // 台湾梅氏鳊 (?????). New name via EOL
Pseudophoxinus elizavetae = Sultan Sazlığı minnow // (Sultan Sazl?g? Minnow)
Pseudophoxinus maeandricus = Sandıklı spring minnow // (Sand?kl? Spring Minnow)
Scardinius elmaliensis = Elmalı rudd // (Elmal? Rudd)
Paraschistura chrysicristinae = Diyarbakır loach // (Diyarbak?r Loach
Pseudecheneis paviei = Jhong jian jhe jhao catfish // (????) 平吻褶鮡 // FIXME: transliteration
Aphanius danfordii = Kızılırmak toothcarp // (matches wikipedia article name). // or Sultan Sazlığı Toothcarp // (Sultan Sazl?g? Toothcarp)
Cambarus setosus = Βristly cave crayfish // (?ristly Cave Crayfish)
Cyclothone braueri = Bent-tooth lightfish // Bent - tooth lightfish
Conus flavidus = Yellow Pacific cone // Yellow Pacific cone, Golden - Yellow cone
Oncorhynchus gilae = Gila trout // Gila or Apache Trout, Gila Trout
Suncus zeylanicus = Ceylon jungle shrew // semicolon: Ceylon Jungle Shrew, Jungle Shrew, Sri Lankan Shrew Shrew; Sri Lankan Shrew)
Cypripedium plectrochilum = ram's head lady slipper // The Spurred-Lip Cypripedium - Ram's Head Lady Slipper
Cordylus tasmani = Tasman's girdled lizard // listed as: Tasman's girlded lizard
Palleon nasus = elongate leaf chameleon // listed as Eiongate leaf chameleon
Speocirolana thermydromis = Speocirolana thermydronis // typo in scientific name (todo: better verb than '=' for this)

// IUCN caps-only changes
Typhlops hectus = Tiburon Peninsula blindsnake // difficult caps
Stenodactylus doriae = Middle Eastern short-fingered gecko
Cycas tropophylla = Ha Long cycad // can't guess that capitalization
Cambarus bouchardi = Big South Fork crayfish // capitalization

// mammal groups/ranks
Cetartiodactyla includes dolphins, whales and even-toed ungulates
Cetartiodactyla force-split true // Tylopoda, Artiofabula (so includes Baiji and Pygmy hog)
// Chiroptera force-split true // micro and macro
Cetartiodactyla split-off Cetacea
//Cetartiodactyla below Ungulata // TODO: wasn't created.. needs rank.. also should be hidden (don't need an extra heading level, just keep the entries together)
//Perissodactyla below Ungulata
Ungulata = ungulate ! ungulates
Primates = primate ! primates
Perissodactyla = odd-toed ungulate ! odd-toed ungulates
Cheirogaleidae = cheirogaleid
Hominidae = great ape ! great apes // note: Hominid refers to humans and relatives of humans closer than chimpanzees
Hylobatidae = gibbon ! gibbons
// Caviidae = cavy family // just 'cavy' is ambiguous
// Habromys schmidlyi = Schmidly's deer mouse // but shares a name with Peromyscus schmidlyi ??
//Echimyidae = echimyid
Echimyidae = Neotropical spiny rat // and their fossil relatives
Muridae = murid ! murids
Bovidae = bovid ! bovids
Giraffidae = giraffid ! giraffids
Suidae = suid ! suids
// Muridae force-split true // maybe.. not really needed
Proboscidea = proboscidean ! proboscideans
Cetartiodactyla = cetartiodactyl ! cetartiodactyls
Pilosa comprises anteaters and sloths
Lemuroidea = lemur ! lemurs
Marsupialia = marsupial ! marsupials
Pitheciidae includes the titis, saki monkeys and uakaris
Pronolagus saundersiae = Hewitt's red rock hare // split from Pronolagus rupestris (was: Pronolagus rupestris saundersiae)
Saimiri boliviensis boliviensis = Bolivian squirrel monkey
Oreohelix strigosa goniogyra = carinated striate banded mountain snail // listed as 'Carinated Striate Banded Mntain Snail'

// descriptions
Afrosoricida includes tenrecs and golden moles
Dasyuromorphia includes most of the Australian carnivorous marsupials
// Bovidae comprises cloven-hoofed, ruminant mammals
// Diprotodontia include the kangaroos, wallabies, possums, koala, wombats, and many others // too long. (all marsupial mammals). 
// Eulipotyphla comprises hedgehogs, gymnures, solenodons, desmans, moles, shrew-like moles, and true shrews . // too long, obvious? from species list
// Eulipotyphla includes many hedgehog, mole and shrew species (or something like that) ?
Cricetidae includes true hamsters, voles, lemmings, and New World rats and mice
// Muridae includes true mice and rats, gerbils, and relatives // allies?
// Muridae comprises the house mouse and relatives
Muridae includes mice, rats, gerbils, and relatives
Hylidae includes tree frog species and their allies
Sciuromorpha means squirrel-like // (""Squirrel-like"")
Sciuridae comprises squirrels, chipmunks, marmots, susliks and prairie dogs
Sciuridae = sciurid ! sciurids // note: otherwise creates the heading 'Squirrel' from page title
Castorimorpha means beaver-like
Myomorpha means mouse-like
Anomaluromorpha means anomalure-like
Hystricomorpha means porcupine-like
Lagomorpha includes hares, rabbits, pikas // obvious from list
Lagomorpha comprises rabbits and relatives

//--rodentia (extant families)--
Rodentia = rodent ! rodents

//Aplodontiidae = mountain beaver
Gliridae = dormouse ! dormice

// mammal species
Addax nasomaculatus = addax // monotypic genus is common name
Hippopotamus amphibius = hippopotamus // monotypic genus is common name
Caracal caracal = caracal  // monotypic genus is common name (also called desert lynx)
Soriculus nigrescens = Himalayan shrew // Soriculus is monotypic 
Indri indri = indri // monotypic genus is common name (also called babakoto)
Santamartamys rufodorsalis = red-crested tree rat // Santamartamys is monotypic, but isn't the common name
// Lycaon pictus = African wild dog // temporary (new redirect)
// Monodelphis unistriatus = One-striped opossum  // temporary (new/missing redirect)
// Neomonachus schauinslandi = Hawaiian monk seal  // temporary (new/missing redirect)
// Monachus schauinslandi = Caribbean monk seal // temporary  (new/missing redirect) // aka West Indian seal, sea wolf
// weirdness: [[Dingo]] and [[Canis lupus dingo]] are separate articles with the same taxobox
Dugong dugon = dugong // genus is the name
Lariscus hosei = four-striped ground squirrel  // no article
Lariscus obscurus = Mentawai three-striped squirrel // no article
Lariscus niobe = Niobe ground squirrel // no article
Lariscus insignis = three-striped ground squirrel // no article
Amphinectomys savamis = Ucayali water rat
// Zyzomys palatalis = Carpentarian rock rat // temporary (missing/misspelled redirect)
Lepilemur aeeclis = red-shouldered sportive lemur
Lepilemur septentrionalis = northern sportive lemur // script struggles with comment in taxobox
Rattus villosissimus = long-haired rat
Dendrolagus mayri = Wondiwoi tree-kangaroo // sometimes considered a subsp.
Congosorex phillipsorum = Phillips' Congo shrew
Lynx lynx balcanicus = Balkan lynx // redirect was missing
Mysateles garridoi = Garrido's hutia // redirect was missing
Cebus aequatorialis = Ecuadorian white-fronted capuchin // syn. Cebus albifrons aequatorialis (previously used by iucn)
Sapajus apella margaritae = Margarita Island capuchin
Tarsius bancanus natunensis = Natuna Islands tarsier
Melanomys zunigae = Zuniga's dark rice rat
Pudu puda = southern pudú
Pudu mephistophiles = northern pudú
Peromyscus slevini = Catalina deer mouse // Slevin's mouse
Dipodomys insularis = San Jose Island kangaroo rat // listed as Dipodomys merriami insularis
Dipodomys margaritae = Margarita Island kangaroo rat // listed as Dipodomys merriami margaritae
Kobus leche anselli = Upemba lechwe // wikipedia: Kobus anselli (from Upemba wetlands)
Balaenoptera musculus musculus = northern blue whale
Balaenoptera musculus intermedia = southern blue whale
// Balaenoptera musculus brevicauda = North Atlantic blue whale // has an article
Tokudaia tokunoshimensis = Tokunoshima spiny rat // Tokunoshima Island
Pongo pygmaeus morio = Northeast Bornean orangutan
Hapalemur griseus gilberti = Gilbert's bamboo lemur
Daubentoniidae = Daubentoniidae // otherwise becomes 'Aye-ayes' and it's one species monotypic TODO: don't use common name in these special cases.. also Hamerkops
Bdeogale omnivora = Sokoke bushy-tailed mongoose
Sousa teuszii = Atlantic humpback dolphin // only has a genus-level article
Phataginus tetradactyla = long-tailed pangolin // ?? why not found
Diceros bicornis bicornis = southern black rhinoceros
Bassaricyon beddardi = Beddard's olingo // Following ITIS, Wikipedia treats it is a synonym of eastern lowland olingo (Bassaricyon alleni) which is listed separately by IUCN. 
//TODO: check that species don't end up at the same page like Bassaricyon beddardi & Bassaricyon alleni
Monticolomys koopmani = Malagasy mountain mouse
Voalavo gymnocaudus = naked-tailed voalavo
Pygeretmus zhitkovi = greater fat-tailed jerboa // temporary: was not found because page is listed under a synonym: Pygeretmus shitkovi
Pogonomelomys bruijnii = lowland brush mouse // temporary
Bubalus quarlesi = mountain anoa  // Taxobox mashes two species together (TODO: Fix the taxobox / split the articles)
Bubalus depressicornis = lowland anoa
Platanista gangetica minor = Indus river dolphin
Platanista gangetica gangetica = Ganges river dolphin
Crocidura hikmiya = Sri Lankan rain forest shrew // article should probably be renamed, but can't confirm common name (not in EOL or IUCN)

//arthropods
Opiliones = harvestman ! harvestmen
Artemia monica = Mono Lake brine shrimp
Neonemobius eurynotus = California ground cricket
Maxillopoda includes barnacles, copepods and a number of related animals
Malacostraca = malacostracan ! malacostracans // TODO: auto this pattern?
Malacostraca includes crabs, lobsters, crayfish, shrimp, krill, woodlice, and many others
Mictacea = mictacean ! mictaceans
Isopoda = isopod ! isopods 
Amphipoda = amphipod ! amphipods
Decapoda = decapod ! decapods 
Lepidoptera comprises moths and butterflies 
Odonata includes dragonflies and damselflies
Dryococelus australis = Lord Howe Island stick insect
Ostracoda = seed shrimp ! seed shrimps // also ostracods

//fish
Acanthocobitis urophthalmus = banded mountain zipper loach
Belontia signata = Ceylonese combtail
Rasboroides vaterifloris = pearly rasbora
Pethia cumingii = two spot barb
Lampetra spadicea = Chapala lamprey // or Mexican lamprey
Lampetra lanceolata = Turkish brook lamprey

//birds
Sulidae comprises the gannets and boobies
Procellariiformes includes petrels and albatrosses
Gruiformes means crane-like
Podicipediformes = grebe ! grebes
Otidiformes = bustard ! bustards
Psittaciformes = parrot ! parrots
// Suliformes means booby-like // maybe?
Pelecaniformes means pelican-like
// Galliformes = galliforms
Bucerotiformes includes hornbills, hoopoe and wood hoopoes
Accipitriformes includes most of the diurnal birds of prey
Anseriformes means goose-like
Cathartiformes =  New World vulture ! New World vultures //NOTE: also includes extinct teratornithidae
Strigiformes = owl ! owls
// Coraciiformes means raven-like // true but it's a misnomer, and not helpful
Coraciiformes includes kingfishers and bee-eaters // also rollers,  motmots, and todies
Passeriformes = passerine ! passerines
Cuculiformes = cuckoo ! cuckoos 
//Piciformes includes woodpeckers and allies
Piciformes means woodpecker-like
Phapitreron frontalis = Cebu brown-dove // split from P. amethystinus
Crax pinima = Belem curassow // split from Crax fasciolata. From Belém (Brazil)
Turnix novaecaledoniae = New Caledonian buttonquail
Ceyx sangirensis = Sangihe dwarf kingfisher // = Ceyx fallax sangirensis
Otus feae = Annobon scops-owl // split from Otus senegalensis. = Otus senegalensis feae // possibly unneeded now
Scleroptila whytei = Whyte's Francolin
Guttera verreauxi = Western crested guineafowl

// avoid eponyms 
Zaglossus attenboroughi = Cyclops long-beaked echidna // Sir David's long-beaked echidna
Lepilemur jamesorum = Manombo sportive lemur // James' sportive lemur
Microcebus mamiratra = Nosy Be mouse lemur // Claire's mouse lemur
Lepilemur fleuretae = Andohahela sportive lemur // Fleurete's sportive lemur
Callicebus barbarabrownae = blond titi monkey
Cephalorhynchus hectori maui = popoto // Maui's dolphin (from te Ika-a-Māui, the Māori word for New Zealand's North Island)

//mammal subspecies
Tragelaphus eurycerus isaaci = mountain bongo
Tragelaphus eurycerus eurycerus = lowland bongo
Tragelaphus derbianus derbianus = western giant eland
Balaenoptera musculus intermedia = southern blue whale
Cercopithecus mitis schoutedeni = Schouteden's blue monkey
Cercopithecus mitis zammaronoi = Zammarano's monkey
//TODO:
// Presbytis chrysomelas chrysomelas
// Presbytis chrysomelas cruciger
// Presbytis potenziani potenziani
Procolobus pennantii bouvieri = Bouvier's red colobus
Procolobus pennantii pennantii =  Bioko red colobus 
Procolobus pennantii epieni = Niger Delta red colobus
Simias concolor concolor = Pagai Island pig-tailed snub-nosed monkey
Simias concolor siberu = Siberut Island pig-tailed snub-nosed monkey
Trachypithecus cristatus vigilans = Natuna Islands silvery lutung // debated subspecies
// Trachypithecus poliocephalus leucocephalus = white-headed black langur // may be ambiguous common name?
// Trachypithecus poliocephalus poliocephalus // not sure which are ambiguous: Cat Ba Hooded Black Leaf Monkey, Cat Ba Langur, Golden-headed Langur, Tonkin Hooded Black Langur
Varecia variegata variegata = pied black-and-white ruffed lemur
Varecia variegata editorum = southern black-and-white ruffed lemur
Varecia variegata subcincta = northern black-and-white ruffed lemur
Cebus albifrons aequatorialis = Ecuadorian white-fronted capuchin
Cebus albifrons trinitatis = Trinidad white-fronted capuchin 
Cebus albifrons versicolor = varied white-fronted capuchin
Neophocaena asiaeorientalis asiaeorientalis = Yangtze finless porpoise
Nomascus concolor concolor = Tonkin black crested gibbon
Nomascus concolor furvogaster = West Yunnan black crested gibbon
Nomascus concolor jingdongensis = Central Yunnan black crested gibbon 
Nomascus concolor lu = Laotian black crested gibbon
Cephalopachus bancanus natunensis = Natuna Islands tarsier
Prionailurus bengalensis rabori = Visayan leopard cat // temp (redirect was missing)
Cercopithecidae = Old World monkey ! Old World monkeys
Platyrrhini = New World monkey ! New World monkeys
Pholidota = pangolin ! pangolins
Trachypithecus poliocephalus leucocephalus = white-headed black langur
Trachypithecus poliocephalus poliocephalus = Cat Ba langur
Presbytis chrysomelas cruciger = tricolored langur
Trichechus manatus latirostris = Florida manatee
Trichechus manatus manatus = Antillean manatee // or Caribbean manatee
Pogonomys fergussoniensis = D'Entrecasteaux Archipelago tree mouse ! D'Entrecasteaux Archipelago tree mice
Dama mesopotamica = Persian fallow deer // redirects to Dama dama mesopotamica
Oryzomys gorgasi = Gorgas's rice rat
Eliurus petteri = Petter's tuft-tailed rat
Presbytis potenziani potenziani = golden-bellied Mentawai Island langur // from IUCN red list, but can't get automatic capitalizational yet

// lemurs
Archaeolemuridae below Lemuroidea : superfamily //extinct
Cheirogaleidae below Lemuroidea : superfamily
Daubentoniidae below Lemuroidea : superfamily 
Indriidae below Lemuroidea : superfamily
Lemuridae below Lemuroidea : superfamily 
Lepilemuridae below Lemuroidea : superfamily // Sportive lemur
Megaladapidae below Lemuroidea : superfamily // extinct
Palaeopropithecidae below Lemuroidea : superfamily //extinct
Lemuroidea = lemur

// marsupials
Didelphimorphia below Marsupialia : infraclass
Paucituberculata below Marsupialia : infraclass
Microbiotheria below Marsupialia : infraclass
Yalkaparidontia below Marsupialia : infraclass // extinct
Dasyuromorphia below Marsupialia : infraclass // quolls, thylacines, devils, dunnarts, antechinuses
Peramelemorphia below Marsupialia : infraclass 
Notoryctemorphia below Marsupialia : infraclass
Diprotodontia below Marsupialia : infraclass // koalas etc

// New World monkeys (Platyrrhini)
Callitrichidae below Platyrrhini : parvorder // marmosets and tamarins
Cebidae below Platyrrhini : parvorder  // capuchins and squirrel monkeys
Aotidae below Platyrrhini : parvorder // night or owl monkeys (douroucoulis)
Pitheciidae below Platyrrhini : parvorder // titis, sakis and uakaris
Atelidae below Platyrrhini : parvorder  // howler, spider, woolly spider and woolly monkeys

// Lorisoidea
Lorisidae below Lorisoidea : superfamily // lorises, pottos, and angwantibos
Galagidae below Lorisoidea : superfamily // galagos

// Fish groups
Agnatha below Fish : paraphyletic-group
Actinopterygii below Fish : paraphyletic-group
Cephalaspidomorphi below Fish : paraphyletic-group // lampreys and fossil species (disputed, but used by IUCN for lampreys)
Chondrichthyes below Fish : paraphyletic-group
Placodermi below Fish : paraphyletic-group
Sarcopterygii below Fish : paraphyletic-group
Myxini below Fish : paraphyletic-group // hagfishes
Fish = fish ! fishes // common name of sorts
Perciformes means perch-like
Cypriniformes includes carps, minnows, loaches and relatives
Chondrichthyes includes sharks, rays, skates, and sawfish // sometimes includes chimaeras
Acipenseriformes includes sturgeons and paddlefishes // as well as some extinct families.
Cyprinodontiformes = toothcarp ! toothcarps // though they are not actually close relatives of the true carps
Syngnathiformes includes the pipefishes and seahorses

// Fish classes
Agnatha = jawless fish ! jawless fishes
Actinopterygii = ray-finned fish ! ray-finned fishes
Chondrichthyes = cartilaginous fish ! cartilaginous fishes
Placodermi = armoured fish ! armoured fishes // fossil
Sarcopterygii = lobe-finned fish ! lobe-finned fishes
Cobitidae = true loach ! true loaches
Cyprinidae = cyprinid ! cyprinids
Psilorhynchus = mountain carp ! mountain carps
Catostomidae = sucker ! suckers
Osmeriformes includes freshwater smelts and allies
Osmeriformes means smelt-shaped
Percopsiformes comprises trout-perch and its allies
Gonorynchiformes includes milkfish, beaked salmon and allies
Lamniformes = mackerel shark ! mackerel sharks
Rajiformes plural rays and skates // TODO: different syntax for this (and similar items)
Carcharhiniformes = ground shark ! ground sharks
Salmonidae = salmonid ! salmonids
Atheriniformes = silverside ! silversides
Gasterosteiformes includes sticklebacks and relatives
// Gasterosteiformes means bone-bellies-like
Sarotherodon linnellii = unga // or blackbelly tilapia
// Cephalaspidomorphi means head-shield
Cephalaspidomorphi = lamprey ! lampreys // note: contains fossil species which are not lampreys. also disputed.

// fish species
Girardinichthys turneri = highland splitfin
Eptatretus octatrema = eightgill hagfish
Paraschistura chrysicristinae = Diyarbakir loach // Diyarbakır Loach (dotless i on IUCN becomes 'Diyarbak?r loach')

// Frog groups
Aromobatidae = cryptic forest frog
Arthroleptidae = screeching frog // also called squeakers
Craugastoridae = fleshbelly frog
Cycloramphidae == Cycloramphinae // spelled slightly differently on wikipedia (but isn't a redirect to common name)
Dicroglossidae = fork-tongued frog
Eleutherodactylidae = robber frog // also, rain frog (which is ambiguous)
// Eleutherodactylidae means free-toed
Brevicipitidae = rain frog // rain frog is ambiguous
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
Myobatrachidae = Australian water frog ! Australian water frogs // note: called Australian ground frogs on wiki
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
Microgale jenkinsae = Jenkins' shrew tenrec // missing taxobox

Artiodactyla = non-cetacean even-toed ungulate ! non-cetacean even-toed ungulates // not a true clade, but allows spliting of cetaceans and land mammals for display purposes
Hippopotamidae below Artiodactyla : unranked
Moschidae below Artiodactyla : unranked
Tayassuidae below Artiodactyla : unranked
Suidae below Artiodactyla : unranked
Cervidae below Artiodactyla : unranked
Giraffidae below Artiodactyla : unranked
Camelidae below Artiodactyla : unranked
Bovidae below Artiodactyla : unranked
Tragulidae below Artiodactyla : unranked
Antilocapridae below Artiodactyla : unranked

// CETACEA
// IUCN has all Cetecea under CETARTIODACTYLA (order on iucn, unranked on wiki)
// so group them together as unranked (it's an order on wiki)
//TODO: remove ranks which are subordinate to each other (remove the one not used by IUCN)
Cetacea = cetacean ! cetaceans
Balaenidae below Cetacea : unranked // Right whales and bowheadwhale
Ziphiidae below Cetacea : unranked
Ziphidae below Cetacea : unranked
Balaenopteridae below Cetacea : unranked // Rorquals
Eschrichtiidae below Cetacea : unranked 
Cetotheriidae below Cetacea : unranked 
Delphinidae below Cetacea : unranked  // Dolphin
Monodontidae below Cetacea : unranked 
Phocoenidae below Cetacea : unranked  // Porpoises
Physeteridae below Cetacea : unranked  // Sperm whalefamily
Kogiidae below Cetacea : unranked  // – MSW3 treats Kogia as a member ofPhyseteridae
Iniidae below Cetacea : unranked
Lipotidae below Cetacea : unranked // – MSW3 treats Lipotes as a member of Iniidae
Pontoporiidae below Cetacea : unranked // – MSW3 treats Pontoporia as a member of Iniidae
Platanistidae below Cetacea : unranked

// CETACEA: extinct and other cetecea from wikispecies
Aetiocetidae below Cetacea : unranked
Eomysticetidae below Cetacea : unranked
Ambulocetidae below Cetacea : unranked
Basilosauridae below Cetacea : unranked
Pakicetidae below Cetacea : unranked
Protocetidae below Cetacea : unranked
Remingtonicetidae below Cetacea : unranked
// Balaenidae below Cetacea : unranked // double
// Balaenopteridae below Cetacea : unranked // double
// Eschrichtiidae below Cetacea : unranked // double
Neobalaenidae below Cetacea : unranked
// Cetotheriidae below Cetacea : unranked // double
Janjucetidae below Cetacea : unranked

//Chiroptera
Chiroptera = bat ! bats
Nycteridae = slit-faced bat
Hipposideridae = Old World leaf-nosed bat
Natalidae = funnel-eared bat
Mormoopidae includes ghost-faced bats, moustached bats, and naked-backed bats // includes? or complete list? or are they synonyms?
Mystacinidae = New Zealand short-tailed bat // only one genus, two species
Natalida = funnel-eared bat
// Pteralopex flanneryi = greater monkey-faced bat // temporary (redirect was missing)
Rhinolophus hilli = Hill's horseshoe bat
Rhinopomatidae = mouse-tailed bat
Thyropteridae = disc-winged bat
Megadermatidae = false vampire bat

//microbats
Craseonycteridae below Microchiroptera : suborder
Emballonuridae below Microchiroptera : suborder
Furipteridae below Microchiroptera : suborder // just two bat species
Hipposideridae below Microchiroptera : suborder // Old World leaf-nosed bat species
Megadermatidae below Microchiroptera : suborder
Molossidae below Microchiroptera : suborder
Mormoopidae below Microchiroptera : suborder
Mystacinidae below Microchiroptera : suborder
Myzopodidae below Microchiroptera : suborder
Natalidae below Microchiroptera : suborder
Noctilionidae below Microchiroptera : suborder
Nycteridae below Microchiroptera : suborder
Phyllostomidae below Microchiroptera : suborder
Rhinolophidae below Microchiroptera : suborder
Rhinopomatidae below Microchiroptera : suborder
Thyropteridae below Microchiroptera : suborder
// Vespertilionidae below Vespertilionoidea : superfamily
Vespertilionidae below Microchiroptera : suborder
// Vespertilionoidea below Microchiroptera : suborder
// Miniopteridae below Vespertilionoidea : superfamily  // outdated taxonomy from wikipedia
// iucn lists both Vespertilionidae and Miniopteridae as families (under Chiroptera)
Miniopteridae below Microchiroptera : suborder
Miniopteridae = long-fingered bat
// Cistugidae below Vespertilionoidea : superfamily   // outdated: historically been included in family Vespertilionidae
Cistugidae below Microchiroptera : suborder
Cistugidae = wing-gland bat
Neoromicia malagasyensis = Isalo serotine // Isalo = a town name. alt: Peterson's pipistrelle
Microchiroptera = microbat ! microbats // not found uncapitalized on page enough times to register

//bat species
Nyctophilus nebulosus = New Caledonian long-eared bat // temporary (new redirect)
Eumops floridanus = Florida bonneted bat
// Pteronotus paraguanensis = ??? no common name // syn: Pteronotus parnellii paraguanensis. known only Peninsula de Paraguaná, Venezuela (Paraguaná Peninsula)
Emballonura tiavato = western sheath-tailed bat
Pteronotus p0araguanensis = Paraguana mustached bat // Refers to Paraguaná (Venezuela)
Rhinolophus ziama = Ziama horseshoe bat // temporary (article moved to common name)


//rodent groups
Abrocomidae below Hystricomorpha : suborder
Anomaluridae below Anomaluromorpha : suborder
Aplodontiidae below Sciuromorpha : suborder
Bathyergidae below Hystricomorpha : suborder
Capromyidae below Hystricomorpha : suborder
Castoridae below Castorimorpha : suborder
Caviidae below Hystricomorpha : suborder
Chinchillidae below Hystricomorpha : suborder
Cricetidae below Myomorpha : suborder
Ctenodactylidae below Hystricomorpha : suborder
Ctenomyidae below Hystricomorpha : suborder
Cuniculidae below Hystricomorpha : suborder
Dasyproctidae below Hystricomorpha : suborder
Diatomyidae below Hystricomorpha : suborder
Dinomyidae below Hystricomorpha : suborder
Dipodidae below Myomorpha : suborder
Echimyidae below Hystricomorpha : suborder
Erethizontidae below Hystricomorpha : suborder
Geomyidae below Castorimorpha : suborder  // Historically below Myomorpha
Gliridae below Sciuromorpha : suborder // Historically below Myomorpha
Heteromyidae below Castorimorpha : suborder // Historically below Myomorpha
Hystricidae below Hystricomorpha : suborder
Muridae below Myomorpha : suborder
Myocastoridae below Hystricomorpha : suborder
Nesomyidae below Myomorpha : suborder
Octodontidae below Hystricomorpha : suborder
Pedetidae below Anomaluromorpha : suborder
Petromuridae below Hystricomorpha : suborder
Platacanthomyidae below Myomorpha : suborder
Sciuridae below Sciuromorpha : suborder
Spalacidae below Myomorpha : suborder
Thryonomyidae below Hystricomorpha : suborder

Spalacidae = spalacid ! spalacids
Nesomyidae = nesomyid ! nesomyids
Cricetidae = cricetid ! cricetids
Dipodidae = dipodid ! dipodids

// rodent families not listed by iucn, but included anyway (though many more extinct families not included)
Parapedetidae below Anomaluromorpha : suborder
Zegdoumyidae below Anomaluromorpha : suborder // (extinct) 'sometimes placed in the Anomaluromorpha'
Eutypomyidae below Castorimorpha : suborder // extinct
Rhizospalacidae below Castorimorpha : suborder // extinct
Eomyidae below Castorimorpha : suborder  // extinct
Calomyscidae below Myomorpha : suborder

//birds
Columbiformes plural pigeons and doves
Columbidae plural pigeons and doves
Dromaius minor = King Island emu // redirects to ssp syn. (Dromaius novaehollandiae minor)

//mollusks, Mollusca
Stylommatophora includes the majority of land snails and slugs

";

        private static TaxaRuleList _instance;
        public static TaxaRuleList Instance() {
            if (_instance == null) {
                _instance = new TaxaRuleList();
                _instance.Compile();
            }

            return _instance;
        }

        string rules;

        // compiled:
        public Dictionary<string, TaxonRules> records = new Dictionary<string, TaxonRules>();

        public HashSet<String> BinomAmbig;
        public HashSet<String> InfraAmbig;
        public HashSet<String> WikiPageAmbig; // pages that are pointed to by multiple species
        
        // see also: https://en.wikipedia.org/wiki/User:Beastie_Bot/Redirects_to_same_title
        public Dupes WikiSpeciesDupes; // Page names that link to same species
        public Dupes WikiHigherDupes; // Page names that link to the same higher taxon
        public Dictionary<string, string> Caps = new Dictionary<string, string>(); // lowercase, corrected case. For IUCN Red List common names

        //public Dictionary<string, TaxonRules.Field> fields = new Dictionary<string, TaxonRules.Field>(); // const

        public TaxaRuleList() {
            rules = GeneralRules;

            //     public enum TaxonDisplayField { None, commonName, commonPlural, forcesplit, splitOff, below, includes, comprises, means, wikilink }

            //fields["force-split"] = TaxonRules.Field.forcesplit;
            //fields["="] = TaxonRules.Field.commonName;
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
                    //string[] parts = SplitAndAddToDictionary(line, " = ", lineNumber, taxonCommonName);
                    string[] parts = SplitAndAddToRecord(line, " = ", "!", lineNumber, TaxonRules.Field.commonName, TaxonRules.Field.commonPlural);

                    // warn if -s ending
                    if (parts != null) {
                        string common = parts[1];
                        if (common.EndsWith("s")
                            && !common.Contains("species")
                            && !common.EndsWith("fishes")
                            && !common.EndsWith("colobus") 
                            && !common.EndsWith("hippopotamus")
                            && !common.EndsWith("rhinoceros") 
                            && !common.EndsWith("pogonomys") // (temporary)
                            ) {
                            Warning(lineNumber, line, "Common name may be plural (ends with 's'): " + common);
                        }
                    }

                } else if (line.Contains("force-split")) {
                    string split = line.Substring(0, line.IndexOf("force-split"));
                    SplitAndAddToRecord(line, " force-split ", lineNumber, TaxonRules.Field.forcesplit);

                } else if (line.Contains(" plural ")) {
                    //records.GetOrDefault(SplitAndAddToRecord(line, " plural ", lineNumber, TaxonRules.Field.commonPlural);
                    SplitAndAddToRecord(line, " plural ", lineNumber, TaxonRules.Field.commonPlural);

                } else if (line.Contains(" adj ")) {
                    SplitAndAddToRecord(line, " adj ", lineNumber, TaxonRules.Field.adj);

                } else if (line.Contains(" split-off ")) {
                    SplitAndAddToRecord(line, " split-off ", lineNumber, TaxonRules.Field.splitoff);

                } else if (line.Contains(" below ")) {
                    //SplitAndAddToRecord(line, " below ", lineNumber, TaxonRules.Field.below);
                    SplitAndAddToRecord(line, " below ", ":", lineNumber, TaxonRules.Field.below, TaxonRules.Field.belowRank);
                    //TODO: error if rank missing

                } else if (line.Contains(" includes ")) {
                    SplitAndAddToRecord(line, " includes ", lineNumber, TaxonRules.Field.includes);

                } else if (line.Contains(" comprises ")) {
                    SplitAndAddToRecord(line, " comprises ", lineNumber, TaxonRules.Field.comprises);

                } else if (line.Contains(" means ")) {
                    SplitAndAddToRecord(line, " means ", lineNumber, TaxonRules.Field.means);

                } else if (line.Contains(" wikilink")) {
                    //SplitAndAddToRecord(line, " wikilink ", lineNumber, TaxonRules.Field.wikilink);
                    SplitAndAddToRecord(line, " wikilink ", lineNumber, TaxonRules.Field.wikilink);
                }
            }
        }

        string[] SplitAndAddToRecord(string line, string seperator, int lineNumber, TaxonRules.Field setField = TaxonRules.Field.None) {
            var parts = line.Split(new string[] { seperator }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) {
                Error(lineNumber, line,
                    string.Format("'{0}' statement missing arguments. Needs something on either side: {1}", seperator, line));
                return null;
            } else {
                parts[0] = parts[0].Trim();
                parts[1] = parts[1].Trim();
                string taxonString = parts[0];
                string fieldValue = parts[1];

                if (setField != TaxonRules.Field.None) {
                    TaxonRules record = GetOrCreateRecord(taxonString);

                    record[setField] = fieldValue;
                }

                return parts;
            }
        }

        //string[] SplitAndAddToDictionaries(string line, string seperator1, string seperator2, int lineNumber, Dictionary<string, string> addToDictionary1 = null, Dictionary<string, string> addToDictionary2 = null) {
        string[] SplitAndAddToRecord(string line, string seperator1, string seperator2, int lineNumber, TaxonRules.Field setField1 = TaxonRules.Field.None, TaxonRules.Field setField2 = TaxonRules.Field.None) {
            
            var parts = line.Split(new string[] { seperator1 }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) {
                Error(lineNumber, line,
                    string.Format("'{0}' statement missing arguments. Needs something on either side of '{1}'", line, seperator1));
                return null;
            } else {
                var subparts = parts[1].Split(new string[] { seperator2 }, 2, StringSplitOptions.RemoveEmptyEntries);

                if (subparts.Length == 0) {
                    return null; // error?

                } else if (subparts.Length == 1) { // if (parts.Length == 1) {
                    parts[0] = parts[0].Trim();
                    parts[1] = parts[1].Trim();

                    string taxonString = parts[0];
                    string fieldValue = parts[1];

                    if (setField1 != TaxonRules.Field.None) {
                        TaxonRules record = GetOrCreateRecord(taxonString);

                        record[setField1] = fieldValue;
                    }

                    return parts;

                } else {
                    parts[0] = parts[0].Trim();
                    string taxonString = parts[0];

                    if (subparts.Length != 2) {
                        Error(lineNumber, line,
                            string.Format("'{0}' statement has too many arguments or something. Should have one of each separator '{1}' and '{2}'", line, seperator1, seperator2));
                        return null;
                    }

                    subparts[0] = subparts[0].Trim();
                    subparts[1] = subparts[1].Trim();

                    if (setField1 != TaxonRules.Field.None) {
                        TaxonRules record = GetOrCreateRecord(taxonString);
                        record[setField1] = subparts[0];

                    }
                    if (setField2 != TaxonRules.Field.None) {
                        TaxonRules record = GetOrCreateRecord(taxonString);
                        record[setField2] = subparts[1];
                    }

                    return new string[] { taxonString, subparts[0], subparts[1] };
                }
            }
        }

        TaxonRules GetOrCreateRecord(string taxonString) {
            TaxonRules record = null;
            records.TryGetValue(taxonString, out record);
            if (record == null) {
                record = new TaxonRules();
                records[taxonString] = record;
            }
            return record;
        }

        void Warning(int lineNumber, string line, string warning) {
            Console.Error.WriteLine("Warning on line {0}: {1}", lineNumber, warning);
            Console.Error.WriteLine("Line {0}: {1}", lineNumber, line);
        }

        void Error(int lineNumber, string line, string error) {
            Console.Error.WriteLine("Error on line {0}: {1}", lineNumber, error);
            Console.Error.WriteLine("Line {0}: {1}", lineNumber, line);
        }

        public TaxonRules GetDetails(string taxon) {
            TaxonRules details = null;
            records.TryGetValue(taxon, out details);
            return details;
        }

        public TaxonRules GetOrCreateDetails(string taxon) {
            TaxonRules details = null;
            if (records.TryGetValue(taxon, out details)) { 
                return details;
            } else { 
                details = new TaxonRules();
                records[taxon] = details;
                return details;
            }
        }
    }
}
