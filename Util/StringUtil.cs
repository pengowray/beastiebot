//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace beastie
{
	public static class StringUtil
	{

	
		public static string JoinStrings(this IEnumerable<object> source, 
		                                         string separator)
		{
			StringBuilder builder = new StringBuilder();
			bool first = true;
			//foreach (T element in source)
			foreach (object element in source)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					builder.Append(separator);
				}
				builder.Append(element);
			}
			return builder.ToString();
		}

		/***
		 * Uppercases the first character, and lowercases the rest
		 */
		public static string TitleCaseOneWord(this string word) {
			if (word.Length > 1) {
				return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
			} else {
				return word.ToUpperInvariant();
			}
		}

		/***
		 * Uppercases the first character and leaves the rest the same
		 */
		public static string UpperCaseFirstChar(this string word) {
			if (word.Length > 1) {
				return char.ToUpperInvariant(word[0]) + word.Substring(1);
			} else {
				return word.ToUpperInvariant();
			}
		}

        public static string UpperCaseFirstChar(this string word, bool ifTrue) {
            if (!ifTrue)
                return word;

            if (word.Length > 1) {
                return char.ToUpperInvariant(word[0]) + word.Substring(1);
            } else {
                return word.ToUpperInvariant();
            }
        }

        public static string NormalizeSpaces(this string value) {
			return Regex.Replace(value, @"\s+", " ");
		}

		public static string NewspaperNumber(this int number) {
			var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

			if (number >= 0 && number <= 10) {
				return unitsMap[number];
			} else if (number >= 10000) {
                // e.g. 14,462
                return number.ToString("N0");
            } else {
                return number.ToString();
			}

		}

		/**
		 * Also escapes control codes. Must be unescaped with CsvUnescapeSafe 
		 */
		public static string CsvEscapeSafe(this string s) {
			s = s.EscapeToCSharpLiteral();
			return s.CsvEscape(true);
		}

		public static string CsvUnescapeSafe(this string s) {
			return s.CsvUnescape().UnescapeCSharpLiteral();
		}

		public static string CsvEscape(this string s, bool quoteRegardless=false) {
			if ( s.Contains( QUOTE ) )
				s = s.Replace( QUOTE, ESCAPED_QUOTE );

			if ( quoteRegardless || s.IndexOfAny( CHARACTERS_THAT_MUST_BE_QUOTED ) > -1 )
				s = QUOTE + s + QUOTE;

			return s;
		}

		public static string CsvUnescape(this string s)
		{
			if ( s.StartsWith( QUOTE ) && s.EndsWith( QUOTE ) )
			{
				s = s.Substring( 1, s.Length - 2 );

				if ( s.Contains( ESCAPED_QUOTE ) )
					s = s.Replace( ESCAPED_QUOTE, QUOTE );

			}

			return s;
		}
		private const string QUOTE = "\"";
		private const string ESCAPED_QUOTE = "\"\"";
		private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

		// https://stackoverflow.com/questions/10484833/detecting-bad-utf-8-encoding-list-of-bad-characters-to-sniff
		//static Regex _fixableUnicodeRegex = null;
		static Regex CreateFixableUnicodeRegex(Encoding encoding) {
			string specials = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö";
			//specials += "×€‚ƒ„…†‡ˆ‰Š‹ŒŽ‘’“”•–—˜™š›œžŸſ";
			specials += "‚ƒ„…†‡ˆŠ‹ŒŽ‘’“”•–—˜š›œžŸſ";  // removed less likely

			//TODO: make a list of special chars found in normal entries

			List<string> flags = new List<string>();
			//var latin1 = Encoding.GetEncoding("Windows-1252"); // ("iso-8859-1");
			//foreach (Encoding encoding in new Encoding[] { Encoding.GetEncoding("Windows-1252"), Encoding.GetEncoding("macintosh") } ) {
			foreach (char c in specials) {
				string s = c.ToString();
				string interpretedAsLatin1 = encoding.GetString(Encoding.UTF8.GetBytes(s)).Trim();//take the specials, treat them as utf-8, interpret them as latin-1
				if (interpretedAsLatin1.Length > 0)//utf-8 chars made up of 2 bytes, interpreted as two single byte latin-1 chars.
					flags.Add(interpretedAsLatin1);

				string formD = c.ToString().Normalize(NormalizationForm.FormD);
				if (formD == s)
					continue;

				string interpretedAsLatin1D = encoding.GetString(Encoding.UTF8.GetBytes(formD)).Trim();
				if (interpretedAsLatin1D.Length > 0)
					flags.Add(interpretedAsLatin1D);
			}
			//}

			string regex = string.Empty;
			foreach (string s in flags)
			{
				if (regex.Length > 0)
					regex += '|';
				regex += Regex.Escape(s);
			}

			//Console.Error.WriteLine(regex);

			return new Regex("(" + regex + ")");
		}

		//force, if true, try to convert even if no candidate conversion things
		public static string FixUTFCautious(this string data, string encodingName = null, bool force=false) {
			if (encodingName == null) { 
				encodingName = "Windows-1252"; // was iso-8859-1
			}
			Encoding encoding = Encoding.GetEncoding(encodingName);

			Match match = null;
			if (!force) {
				match = CreateFixableUnicodeRegex(encoding).Match(data); //TODO: cache
			}

			if (force || match.Success) {
				return Encoding.UTF8.GetString(encoding.GetBytes(data));//from iso-8859-1 (latin-1) to utf-8
			} else {
				return data;
			}
		}

		//TODO: option to check against CreateFixableUnicodeRegex first
		//TODO: cache mac and win CreateFixableUnicodeRegex() versions
		public static string FixUTFMulti(this string data) {
			string withWin1252 = data.FixUTFCautious("Windows-1252");
			string withMacintosh = data.FixUTFCautious("macintosh");
			if (data.Length <= withWin1252.Length && data.Length <= withMacintosh.Length)
				return data;

			if (withMacintosh.Length < withWin1252.Length)
				return withMacintosh;

			return withWin1252;
		}


		public static string FlipCodepageToWin(this string data) {
			return data.FlipCodepage(Encoding.GetEncoding(10000), Encoding.GetEncoding(1252));
		}

		public static string FlipCodepageToMac(this string data) {
			return data.FlipCodepage(Encoding.GetEncoding(1252), Encoding.GetEncoding(10000));
		}

		public static string FlipCodepage(this string data, Encoding from, Encoding to) {
			var bytes = from.GetBytes(data);
			return to.GetString(bytes);
		}

		public static string FindEncoding(this string data, string shouldbe, bool showAll = false) {
			foreach (var enc in Encoding.GetEncodings()) {
				string fix = Encoding.UTF8.GetString(enc.GetEncoding().GetBytes(data));//from whatever to utf-8
				if (fix == shouldbe) {
					Console.WriteLine("encoding fixes it: " + enc.Name + " -- " + enc.CodePage + " -- " + enc.DisplayName);
					//return enc.Name;
				} else if (showAll) {
					Console.WriteLine("encoding:" + enc.Name + " " + fix);
				}
			}
			return null;
		}

		// unlikely to fix anything. mostly a line noise generator
		public static string FixUTFReverse(this string data, string encodingName = null) { // , bool force=false
			if (encodingName == null) { 
				encodingName = "Windows-1252"; // was iso-8859-1
			}
			Encoding encoding = Encoding.GetEncoding(encodingName);

			return encoding.GetString(Encoding.UTF8.GetBytes(data));
			//return Encoding.UTF8.GetString(encoding.GetBytes(data));//from iso-8859-1 (latin-1) to utf-8
		}

		//https://stackoverflow.com/questions/12309104/how-to-print-control-characters-in-console-window
		public static string EscapeToCSharpLiteral(this string str) {
			StringBuilder sb = new StringBuilder();
			foreach(char c in str)
				switch(c)
			{
			case '\'': case '"': case '\\':
				sb.Append(c.EscapeToCSharpLiteral());
				break;
			default:
				if(char.IsControl(c))
					sb.Append(c.EscapeToCSharpLiteral());
				else
					sb.Append(c);
				break;
			}
			return sb.ToString();
		}

		public static string EscapeToCSharpLiteral(this char chr) {
			switch(chr)
			{//first catch the special cases with C# shortcut escapes.
			case '\'':
				return @"\'";
			case '"':
				return "\\\"";
			case '\\':
				return @"\\";
			case '\0':
				return @"\0";
			case '\a':
				return @"\a";
			case '\b':
				return @"\b";
			case '\f':
				return @"\f";
			case '\n':
				return @"\n";
			case '\r':
				return @"\r";
			case '\t':
				return @"\t";
			case '\v':
				return @"\v";
			default:
				//we need to escape surrogates with they're single chars,
				//but in strings we can just use the character they produce.
				if(char.IsControl(chr) || char.IsHighSurrogate(chr) || char.IsLowSurrogate(chr))
					return @"\u" + ((int)chr).ToString("X4");
				else
					return new string(chr, 1);
			}
		}

		// https://stackoverflow.com/questions/2661169/how-can-i-unescape-and-reescape-strings-in-net
		public static string UnescapeSimple(this string txt)
		{
			if (string.IsNullOrEmpty(txt)) { return txt; }
			StringBuilder retval = new StringBuilder(txt.Length);
			for (int ix = 0; ix < txt.Length; )
			{
				int jx = txt.IndexOf('\\', ix);
				if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
				retval.Append(txt, ix, jx - ix);
				if (jx >= txt.Length) break;
				switch (txt[jx + 1])
				{
				case 'n': retval.Append('\n'); break;  // Line feed
				case 'r': retval.Append('\r'); break;  // Carriage return
				case 't': retval.Append('\t'); break;  // Tab
				case '\\': retval.Append('\\'); break; // Don't escape
				default:                                 // Unrecognized, copy as-is
					retval.Append('\\').Append(txt[jx + 1]); break;
				}
				ix = jx + 2;
			}
			return retval.ToString();
		}

	}
}

