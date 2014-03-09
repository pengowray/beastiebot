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

		private string _pos = null; // (empty string), NOUN, ADJ, etc
		public string pos {
			get { 
				if (_pos == null) Clean();
				return _pos;
			}
		}

		public bool hasPos {
			get {
				return (pos != "");
			}

		}

		public bool isCanonicalUnchangedCase {
			get {
				return (cleanedLowercase == raw);
			}
		}

		public string cleanedLowercase {
			get {
				return cleaned.ToLowerInvariant();
			}
		}

		public bool isCanonicalLowercase {
			get {
				return (cleaned == raw);
			}
		}

		public string stemmedUnchangedCase {
			get {
				return Stem(cleaned);
			}
		}

		public string stemmedNormalized {
			get {
				return Stem(cleaned).ToLowerInvariant(); // CultureInfo.InvariantCulture;
			}
		}

		public Lemma(string raw) {
			this._raw = raw;
		}

		private void Clean() {
			// removes trailing POS e.g. atavic_ADJ attaccato_DET
			// removes periods and anything after: afternoon.we anything.there
			// removes trailing numbers or symbols e.g. ate' atoms.1 attitude.8_NOUN αt_. avow_ account31
			// removes trailing 'll or 's

			// doesn't make lowercase -- use Lower()
			// doesn't stem -- use Stem()
			
			// note, allow?: astronomy.com
			// todo: slash letter/number: atoms/m3
			// note: split joined words? architectengineering

			string lemma = raw;
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
			
			lemma = Regex.Replace(lemma, @"[0-9\/\-\.\,\d\']*$", ""); // remove trailing numbers, slashes, commas, periods, apostrophe
			
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

			_cleaned = lemma;
			_pos = pos;
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

