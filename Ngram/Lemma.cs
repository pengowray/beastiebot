//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace beastie
{

	public class Lemma
	{
		//TODO: replace with Paice/Husk Stemmer (modified Lancaster), which is more agressive
		 static SF.Snowball.Ext.EnglishStemmer eng = new SF.Snowball.Ext.EnglishStemmer();

		private bool _fromWikt = false; // false = from ngrams, true = from wiktionary

		private string _raw;
		public string raw { 
			get { return _raw; }
		}

		private string _cleaned = null; // noise removed, see Clean()
		public string cleaned {
			get { 
				if (_cleaned == null) Clean();
				return _cleaned;
			}
		}

		/*
		// _pos is broken by multi-word lemmas, which can have multiple POS, e.g. BOHEMIAN_NOUN CLUB_NOUN
		
		private string _pos = null; // (empty string), NOUN, ADJ, etc
		public string pos {
			get { 
				if (_pos == null) Clean();
				return _pos;
			}
		}
		*/
		private bool _hasPos = false;
		public bool hasPos {
			get {
				if (_cleaned == null) Clean();
				return _hasPos;
			}
		}


		public bool isCanonical {
			get {
				return (cleaned == raw);
			}
		}

		public string cleanedLowercase {
			get {
				return cleaned.ToLowerInvariant();
			}
		}

		public bool isCanonicalLowercase {
			get {
				return (cleanedLowercase == raw);
			}
		}

		public string stemmedUnchangedCase {
			get {
				return Stem(cleaned);
			}
		}

		// check that raw looks like a species (e.g. "Boa constrictor") Must be a two-word lemma for a true result.
		public bool isBinomialCase {
			get {
				if (!isCanonical)
					return false;

				string[] words = raw.Split(new char[]{' '});
				if (words.Length != 2)
					return false;

				if (words[0].Length < 2)
					return false;

				string firstChar = words[0].Substring(0, 1);
				if (firstChar != firstChar.ToUpperInvariant())
					return false;

				string rest = words[0].Substring(1);
				if (rest != rest.ToLowerInvariant())
					return false;

				if (words[1] != words[1].ToLowerInvariant())
					return false;

				return true;
			}
		}

		public string stemmedNormalized {
			get {
				return Stem(cleaned).ToLowerInvariant(); // CultureInfo.InvariantCulture;
			}
		}

		public Lemma(string raw, bool fromWikt = false) {
			this._raw = raw;
			this._fromWikt = fromWikt;
		}

		// create a Lemma from Wiktionary title
		public Lemma(byte[] raw, bool fromWikt) {
			if (fromWikt) {
				this._raw = WiktionaryDatabase.TitleToString(raw);
			} else {
				this._raw = System.Text.Encoding.UTF8.GetString(raw);
			}
			this._fromWikt = fromWikt;
		}

		public static void FliRegexTest() {
			string fliRegex = @"([fli](?=[fli])|(?<=[fli])[fli])";
			string input1 = "oflice/oflices/muflled/stiflly/shuflled/diflerent/diflicult/ofiice/oflicers don't change: igloo/visiting/unwieldly";
			string input2 = "office/offices/muffled/stiffly/shuffled/different/difficult/office/officers don't change: igloo/visiting/unwieldly";
			string output1 = Regex.Replace(input1, fliRegex, "x");
			string output2 = Regex.Replace(input2, fliRegex, "x");

			Console.WriteLine(input1);
			Console.WriteLine(input2);
			Console.WriteLine(output1);
			Console.WriteLine(output2);

			Console.WriteLine("identicical? {0}", (output1 == output2));
		}

		public string ScannoInsensitiveNormalized() {
			//aggressive normalizer to avoid even scanno issues (lowercase)

			string lemma = cleanedLowercase;
			//lemma = Regex.Replace(lemma, @"[0-9\/\-\.\,\d]*$", ""); // remove trailing numbers, slashes, commas, periods, (leave apostrophe)

			// lowercase
			// d => tl  (genderman)
			lemma = lemma.Replace("d","ti");
			// h => b   (hetter)
			lemma = lemma.Replace("h","b");

			// 1/i/I/1 => i // f1nally, lreland, littie... 
			lemma = lemma.Replace("l","i");
			lemma = lemma.Replace("1","i");

			// m <= rn // retuming
			lemma = lemma.Replace("rn","m");

			// i <= r // eveiy, eveiything, diought)
			lemma = lemma.Replace("r","i");

			// replace any combination of two-or-more [fli] characters with i's
			// littie/oflice/oflices/muflled/stiflly/shuflled/diflerent/diflicult/ofiice/oflicers don't change: igloo/visiting/unwieldly 
			// little/office/offices/muffled/stiffly/shuffled/different/difficult/office/officers don't change: igloo/visiting/unwieldly 
			string fliRegex = @"([fli](?=[fli])|(?<=[fli])[fli])";
			lemma = Regex.Replace(lemma, fliRegex, "i");

			//TODO: replace with more elegant:
			//Regex  rx          = new Regex( @"[fli]{2,}" ) ;
			//string lemma = rx.Replace(lemma, m => new string('x',m.Length)) ;


			// c => e // smcllcd 
			lemma = lemma.Replace("c","e");
			// 0 => o  (c0uldn't )
			lemma = lemma.Replace("0","o");
			// v => y
			lemma = lemma.Replace("v","y");
			// 2 => z (puz2led, sei2ed)
			lemma = lemma.Replace("2","z");
			// 6 =>   (fianc6e)
			lemma = lemma.Replace("6","é");
			// 9 =>   (fa9ade)  façade
			lemma = lemma.Replace("9","ç");
			// remove all: - or space or '
			lemma = lemma.Replace("'","");
			lemma = lemma.Replace(" ","");
			lemma = lemma.Replace("-","");

			// nope: 'd => ed  ? fill'd, exceptions: but nobody'd, hell'd, anyway: eye-dialect, not scanno

			// Still making it thru: 
			// thtough (through)
			// fagade
			// noncommittaUy
			// half-do/en (half-dozen)
			// i/i6th, i6th century
			// small gets OCR'd as email


			return lemma;
		}

		private void Clean() {
			string[] words = raw.Split(new char[]{' '});
			for (int i=0; i<words.Length; i++) {
				words[i] = CleanWord(words[i]);
			}

			_cleaned = String.Join(" ", words);

		}

		private string CleanWord(string lemma) {
			// removes trailing _POS e.g. atavic_ADJ attaccato_DET
			// removes periods and anything after: afternoon.we anything.there
			// removes trailing numbers or symbols e.g. ate' (allow: lookin')  atoms.1 attitude.8_NOUN αt_. avow_ account31
			// removes trailing 'll or 's

			// mangles: M-16/M-203 => M-16/M, F/A-18 => F/A

			// hmm: Fiction/Literature/978-0-679-72722-4

			// doesn't make lowercase -- use Lower()
			// doesn't stem -- use Stem()
			
			// note, allow?: astronomy.com, feedback@echo-library.com
			// todo: slash letter/number: atoms/m3
			// note: split joined words? architectengineering

			//string lemma = raw;
			string pos = "";

			if (lemma.Length >= 2)  {
				if (lemma.StartsWith("_")) {
					lemma = lemma.Substring(1, lemma.Length-1);
				}
			}

			if (lemma.Length >= 2)  {
				int underscore = lemma.IndexOf('_',1);
				int lastUnderscore = lemma.LastIndexOf('_');
				if (underscore != -1) {
					pos = lemma.Substring(lastUnderscore);
					lemma = lemma.Substring(0, underscore);
				}
			}
			
			if (lemma.Length >= 2)  {
				int period = lemma.IndexOf('.',1);
				if (period != -1) lemma = lemma.Substring(0, period);
			}
			
			lemma = Regex.Replace(lemma, @"[0-9 _\/\-\.\,\d]*$", ""); // remove trailing numbers, slashes, commas, periods, space, underscore (leave apostrophe)

			//while (! Char.IsLetter(lemma[lemma.Length-2])) {
			//	lemma = lemma.Substring(0, lemma.Length-2);
			//}
			
			string lower = lemma.ToLowerInvariant(); // CultureInfo.InvariantCulture

			if (lower.EndsWith("'s")) {
				lemma = lemma.Substring(0, lemma.Length - "'s".Length);
			}
			
			if (lower.EndsWith("'ll")) {
				lemma = lemma.Substring(0, lemma.Length - "'ll".Length);
			}

			//_cleaned = lemma;
			//_pos = pos; // TODO

			if (pos != null && pos != "") {
				_hasPos = true;
			}

			return lemma;
		}

		public static string Stem(string word) {
			//unattainability -> unattain
			//uglifying -> uglifi
			
			eng.SetCurrent(word);
			eng.Stem();
			return eng.GetCurrent();
		}

	}
}

