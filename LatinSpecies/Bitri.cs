using System;

namespace beastie {
	// A binomial or trinomial, with optional stock/population.
	public class Bitri
	{
		enum Kingdom { None, Plant, Animal, Fungi } // etc...

		//▿

		// Ootaxa: Template:Oobox
		// Ichnotaxa: Template:Ichnobox
		// Excavata (Domain: Eukaryota)
		// Rhizaria (unranked) (Domain: Eukaryota)
		// Chromalveolata (Domain: Eukaryota) (polyphyletic)
		enum Kingdom_Taxobox { None, Animalia, Archaeplastida, Fungi, Chromalveolata, Rhizaria, Excavata, Amoebozoa, Bacteria, Archaea, Viruses, incertae_sedis, Ichnotaxa, Ootaxa }
		enum Kingdom_IUCN { None, Animalia, Bacteria, Chromista, Fungi, Plantae, Protozoa }
		enum Kingdom_COL { None, Animalia, Archaea, Bacteria, Chromista, Fungi, Plantae, Protozoa, Viruses }

		//{{Automatic taxobox
		//{{Speciesbox

		string genus;
		string epithet;
		string infrarank; // infraspecific rank, e.g. subsp. var. 
		string connecting_term; // from above
		string infraspecies; // e.g. subspecies or variety

		string stockpop; // stock/population

		public Bitri() {

		}
	}
}

