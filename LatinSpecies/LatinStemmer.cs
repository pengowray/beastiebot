using System;


// based on
// https://github.com/scherziglu/solr/blob/master/solr-analysis/src/main/java/org/apache/lucene/analysis/la/LatinStemmer.java

//package org.apache.lucene.analysis.la;

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

//import java.util.Arrays;
//import java.util.List;
//import java.util.Locale;
using System.Collections.Generic;

/**
 * Latin Stemmer.
 * based on http://snowball.tartarus.org/otherapps/schinke/intro.html
 * @author Markus Klose
 */
using System.Text;
using System.Globalization;


namespace beastie {
	public class LatinStemmer {
		//TODO queList as txt file an property in schema.xml ???

		/** latin locale - no country specified */
		//private static Locale locale = new Locale("la");
		private static string locale = "la";

		/** list contains words ending with 'que' that should not be stemmed */
		private List<string> queList;

		/**
		 *	default constructor.
		 *
		 *	@author mk 
		 */
		public LatinStemmer() {
			// initialize the queList
			queList = new List<string>( new string[] { "atque", "quoque", "neque", "itaque", "absque", "apsque", "abusque", "adaeque", "adusque", "denique",
				"deque", "susque", "oblique", "peraeque", "plenisque", "quandoque", "quisque", "quaeque",
				"cuiusque", "cuique", "quemque", "quamque", "quaque", "quique", "quorumque", "quarumque",
				"quibusque", "quosque", "quasque", "quotusquisque", "quousque", "ubique", "undique", "usque",
				"uterque", "utique", "utroque", "utribique", "torque", "coque", "concoque", "contorque",
				"detorque", "decoque", "excoque", "extorque", "obtorque", "optorque", "retorque", "recoque",
				"attorque", "incoque", "intorque", "praetorque" } );
		}

		// Java's String.valueOf()
		private static string valueOf(char[] data, int offset, int count) {
			return data.ToString().Substring(offset, count); //TODO: copy before substringing?
		}

		// Java's String.valueOf()
		private static string valueOf(string data, int offset, int count) {
			return data.Substring(offset, count);
		}

		/**
		 * check if token ends with 'que' and if it should be stemmed
		 * @author mk
		 * 
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 * @return 
		 * 	current termLength  (termLength - 3' if token ends with 'que'),<br/> if token should not be stemmed return -1
		 */
		public int stemQUE(char[] termBuffer, int termLength) {
			// buffer to token
			string currentToken = valueOf(termBuffer, 0, termLength).ToLowerInvariant(); // locale

			// check if token should be stemmed
			if (queList.Contains(currentToken)) {
				// dont stem the token
				return -1;
			}

			// chekc if token ends with 'que'
			if (currentToken.EndsWith("que")) {
				// cut of 'que'
				return termLength - 3;
			}
			return termLength;
		}

		/**
 		 * @author mk
		 * 
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 * @return
		 * 	termLength after stemming
	     */
		public static string stemAsNoun(char[] termBuffer, int termLength) {
			// buffer to string
			return stemAsNoun( valueOf(termBuffer, 0, termLength) );
		}

		// https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net?lq=1
		static string RemoveDiacritics(string text) 
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		/**
		 * removing known noun suffixe.<br/>
		 * changes to the snowball - additional suffixe: arum, erum, orum, ebus, uum, ium, ei, ui, im
		 */
		public static string stemAsNoun(string noun) {
			// pengo:
			noun = noun.ToLowerInvariant();
			noun = RemoveDiacritics(noun);
			noun = noun.Replace('v', 'u');
			noun = noun.Replace('j', 'i');
			noun = noun.Replace("æ", "ae");
			noun = noun.Replace("œ", "oe");
			noun = noun.Replace("-", ""); // remove dashes (-)
			noun = noun.Replace("?", "");
			noun = noun.Replace("'", "");
			noun = noun.Replace("\"", "");
			noun = noun.Replace("ß", "ss");
			noun = noun.Replace("ȸ", "db");
			noun = noun.Replace("ᵫ","ue");
			// & . numbers
			noun = noun.Trim();

			noun = doNounStem(noun);

			if ((noun.EndsWith("er") && noun.Length >= 4)) {
				// er -> r. (pengo addition)
				noun = valueOf(noun, 0, noun.Length - 2) + "r";
			}

			return noun;
		}

		private static string doNounStem(string noun) {
			int termLength = noun.Length;
			//var termBuffer = noun.ToCharArray();

			// this part is less edited:

			// check longest suffix
			if ((noun.EndsWith("ibus") || noun.EndsWith("arum") || noun.EndsWith("orum") || noun.EndsWith("ebus")) && noun.Length >= 6) { // removed: || noun.EndsWith("erum") 
				return valueOf(noun, 0, termLength - 4);
			} else  if ((noun.EndsWith("ius") || noun.EndsWith("ium") // pengo removed: || noun.EndsWith("uum") 
				|| noun.EndsWith("iae")) // pengo addition
				&& noun.Length >= 5) {
				return valueOf(noun, 0, termLength - 3);
			} else  if ((noun.EndsWith("ae") || noun.EndsWith("am") || noun.EndsWith("as") || noun.EndsWith("em") || noun.EndsWith("es")
				|| noun.EndsWith("ia") || noun.EndsWith("is") || noun.EndsWith("nt") || noun.EndsWith("os") || noun.EndsWith("ud")
				|| noun.EndsWith("um") || noun.EndsWith("us") || noun.EndsWith("ei") || noun.EndsWith("ui") || noun.EndsWith("im")) 
				|| noun.EndsWith("ii") // ii = pengo addition
				&& noun.Length >= 4) {
				return valueOf(noun, 0, termLength - 2);
			} else  if ((noun.EndsWith("a") || noun.EndsWith("e") || noun.EndsWith("i") || noun.EndsWith("o") || noun.EndsWith("u")) && noun.Length >= 3) {
				return valueOf(noun, 0, termLength - 1);
			}

			// stem nothing
			return  valueOf(noun, 0, termLength);
		}

		/**
		 * removing / changing known verb suffixe.<br/>
		 * @author mk
		 * 
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 * @return
		 * 	termLength after stemming
		 */
		public string stemAsVerb(char[] termBuffer, int termLength) {
			// buffer to string
			string verb = valueOf(termBuffer, 0, termLength).ToLowerInvariant(); // locale

			// check suffixe
			if (verb.EndsWith("iuntur") || verb.EndsWith("erunt") || verb.EndsWith("untur") || verb.EndsWith("iunt") || verb.EndsWith("unt")) {
				// 'iuntur' 'erunt' 'untur' 'iunt' 'unt' -> 'i'
				return this.verbSuffixToI(termBuffer, termLength);
			} else  if (verb.EndsWith("beris") || verb.EndsWith("bor") || verb.EndsWith("bo")) {
				// 'beris' 'bor' 'bo' -> 'bi'
				return this.verbSuffixToBI(termBuffer, termLength);
			} else  if (verb.EndsWith("ero") && termLength >= 5) {
				// 'ero' -> 'eri'
				termBuffer[termLength -1] = 'i';
				return valueOf(termBuffer, 0, termLength);
			} else  if ((verb.EndsWith("mini") || verb.EndsWith("ntur") || verb.EndsWith("stis")) && termLength >= 6) {
				// 'mini' 'ntur' 'stis' -> delete
				return valueOf(termBuffer, 0, termLength  - 4);
			} else  if ((verb.EndsWith("mus") || verb.EndsWith("mur") || verb.EndsWith("ris") || verb.EndsWith("sti") || verb.EndsWith("tis") || verb.EndsWith("tur")) && termLength >= 5) {
				// 'mus' 'ris' 'sti' 'tis' 'tur' -> delete
				return valueOf(termBuffer, 0, termLength  - 3);
			} else  if ((verb.EndsWith("ns") || verb.EndsWith("nt") || verb.EndsWith("ri")) && termLength >= 4) {
				// 'ns' 'nt' 'ri' -> delete
				return valueOf(termBuffer, 0, termLength  - 2);
			} else  if ((verb.EndsWith("m") || verb.EndsWith("r") || verb.EndsWith("s") || verb.EndsWith("t")) && termLength >= 3) {
				// 'm' 'r' 's' 't' -> delete
				return valueOf(termBuffer, 0, termLength  - 1);
			}

			// stem nothing
			return valueOf(termBuffer, 0, termLength);
		}	
		/**
		 * general verb suffixe
		 * praesens indikativ aktiv -> o, s, t, mus, tis, (u)nt, is, it, imus, itis
		 * praesens konjunktiv aktiv -> am, as, at, amus, atis, ant, iam, ias, iat, iamus, iatis, iant
		 *
		 * imperfekt indikativ aktiv -> bam,bas,bat,bamus,batis,bant,   ebam,ebas,ebat,ebamus,ebatis,ebant
		 * imperfekt konjunktiv aktiv -> rem,res,ret,remus,retis,rent,   erem,eres,eret,eremus,eretis,erent
		 *	  
		 * futur 1 indikativ aktiv -> bo,bis,bit,bimus,bitis,bunt,   am,es,et,emus,etis,ent,   iam,ies,iet,iemus,ietis,ient
		 * futur 2 indikativ aktiv ->
		 *	  
		 * perfekt indikativ aktiv -> i,isti,it,imus,istis,erunt,
		 * perfekt konjunktiv aktiv -> erim,eris,erit,erimus,eritis,erint
		 *	  
		 * plusquamperfekt indikativ aktiv -> eram,eras,erat,eramus,eratis,erant
		 * plusquamperfekt konjunktiv aktiv -> issem,isses,isset,issemus,issetis,issent
		 */

		// helper methods
		/**
		 * replacing suffix with 'i'
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 * @return
		 * 	stemmed verb
		 */
		private string verbSuffixToI(char[] termBuffer, int termLength) {
			string verb = valueOf(termBuffer, 0, termLength).ToLowerInvariant();
			// 'iuntur' 'erunt' 'untur' 'iunt' 'unt' -> 'i'
			if (verb.EndsWith("iuntur") && termLength >= 8) {
				return valueOf(termBuffer, 0, termLength - 5);
			} else if ((verb.EndsWith("erunt") || verb.EndsWith("untur")) && termLength >= 7) {
				termBuffer[termLength - 5] = 'i';
				return valueOf(termBuffer, 0, termLength - 4);
			} else if (verb.EndsWith("iunt") && termLength >= 6) {;
				return valueOf(termBuffer, 0, termLength - 3);
			} else if (verb.EndsWith("unt") && termLength >= 5) {
				termBuffer[termLength - 3] = 'i';
				return valueOf(termBuffer, 0, termLength - 2);
			} 
			return valueOf(termBuffer, 0, termLength);
		}

		/**
		 * replacing suffix with 'bi'
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 * @return
		 * 	stemmed verb
		 */
		private string verbSuffixToBI(char[] termBuffer, int termLength) {
			string verb = valueOf(termBuffer, 0, termLength).ToLowerInvariant();
			// 'beris' 'bor' 'bo' -> 'bi'
			if (verb.EndsWith("beris") && termLength >= 7) {
				termBuffer[termLength - 4] = 'i';
				return valueOf(termBuffer, 0, termLength - 3);
			} else if (verb.EndsWith("bor") && termLength >= 5) {
				termBuffer[termLength - 2] = 'i';
				return valueOf(termBuffer, 0, termLength - 1);
			} else if (verb.EndsWith("bo") && termLength >= 4) {;
				termBuffer[termLength - 1] = 'i';
				return valueOf(termBuffer, 0, termLength);
			}
			return valueOf(termBuffer, 0, termLength);
		}

		/**
		 * (from LatinStemFilter.java)
		 * 
		 * Replace replace 'v' with 'u' and 'j' with 'i' (case sensitive).
		 * 
		 * @author markus klose
		 * 
		 * @param termBuffer
		 * 	term buffer containing token
		 * @param termLength
		 * 	length of the token
		 */
		private void replaceVJ(char[] termBuffer, int termLength) {
			for (int i = 0; i < termLength; i++) {
				switch(termBuffer[i]) {
				case 'V': termBuffer[i] = 'U'; break;
				case 'v': termBuffer[i] = 'u'; break;
				case 'J': termBuffer[i] = 'I'; break;
				case 'j': termBuffer[i] = 'i'; break;
				}
			}
		}



	}
}