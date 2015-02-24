using System;
using System.Collections;
using System.Collections.Generic;

namespace beastie {

	// a second attempt at a custom Latin stemmer (first was StemGroups)

	public class PengoLatinStemmer
	{
		public new string[] lemmaSuffixes; // count as a lemma (if found when process starts), and then continue with stemming
		public new Dictionary<string, string> nonterminalSuffixes; // replace key with value and keep stemming
		public new Dictionary<string, string> terminalSuffixes; // stop stemming after replacing key with value
		//public new string[] eponymousSuffixes; // put these in their own category because they're annoying (-ii and -i)

		public PengoLatinStemmer() {
			lemmaSuffixes = new string[] { 
				"ceps", // ceps
			};

			terminalSuffixes = new Dictionary<string, string>();

			terminalSuffixes["ceps"] = "ceps"; // TODO: add all Third declension
			terminalSuffixes["cipites"] = "ceps"; // plural m/f
			terminalSuffixes["cipitis"] = "ceps"; // singular m/f/n genative

			terminalSuffixes["ius"] = "ius"; // First/second declension.
			terminalSuffixes["ia"] = "ius";
			terminalSuffixes["ium"] = "ius";
			terminalSuffixes["ii"] = "ius"; // hmm? genative sing m.,   nom m. pl. 
			terminalSuffixes["iae"] = "ius"; // genative sing f.
			terminalSuffixes["iae"] = "ius"; // hmm? genative sing n.
			terminalSuffixes["i"] = "ius"; // locative sing.
			terminalSuffixes["a"] = "ius";
			terminalSuffixes["orum"] = "ius"; // pl. genative (m/f/n)
			terminalSuffixes["os"] = "ius"; // pl. genative (m/f/n)


			var suffixes = new string[] {

				//funny endings
				"llion","llium","lli",
				"ensis", "ense", "us", 
				"ellus", "ella", "ellum",
				"ii",

				//latin declensions: // todo: be more "systematic"
				"o","ines", 
				"er","eri", "ra", "rum", "ri", "ae",
				"is", "re", "e", "ia",
				"os","us",
				"e","ae",
				"es","arum",
				"as","ae",
				"u","ua",
				"es",
				"us", "a", "um", "i", "ae", //, ""
				"x", "ges", "gis", "gum", ""
			} ;
		}
	}
}

