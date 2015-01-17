//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

// TODO: Classes for language families and language scripts and 

namespace beastie
{
	public class Language {
		public string code; // top-level property
		public string[] names;
		public string familyCode; // "family" TODO: get actual family object
		public string[] scriptCodes; //TODO: make actual script objects
		public string langType; // regular, appendix-constructed //TODO: convert to enum?
		public string[] entry_name_from;
		public string[] entry_name_to;
		public string translitModule;

		public string canonicalName {
			get {
				if (names == null || names.Length == 0) return null;
				return names[0];
			}
		}
		public string categoryName {
			get {
				return (canonicalName.EndsWith("language", StringComparison.InvariantCultureIgnoreCase) ? canonicalName : canonicalName + " language");
			}
		}
		public bool hasScripts { // true if there are scripts listed (not just "None").
			get {
				return (scriptCodes != null && scriptCodes.Length >= 1 && scriptCodes[0] != null && scriptCodes[0] != "" && scriptCodes[0] != "None");
			}
		}

	}

	public class WiktionaryData
	{
		public Dictionary<string, Language> codeIndex;
		public Dictionary<string, Language> nameIndex;
		public Dictionary<string, Language> catnameIndex;

		static private WiktionaryData _singleton;
		public static WiktionaryData Instance(string lang="en") {
			if (_singleton == null) _singleton = new WiktionaryData();
			return _singleton;
		}

		public WiktionaryData()
		{
			
			//TODO: read directly from Module:JSON_Export {{#invoke:JSON data|export_families}}

			// see also: 
			//TODO: singleton

			//WebClient client = new WebClient();
			//Stream stream = client.OpenRead("http://api.kazaa.com/api/v1/search.json?q=muse&type=Album");
			//StreamReader reader = new StreamReader(stream);

			codeIndex = new Dictionary<string, Language>();
			nameIndex = new Dictionary<string, Language>();
			catnameIndex = new Dictionary<string, Language>();

			string filename = @"D:\ngrams\datasets-wiki\JSON_export_languages.txt";
			using (StreamReader reader = new StreamReader(filename, System.Text.Encoding.UTF8)) {
				string json = reader.ReadToEnd();
				JToken root = JObject.Parse(json);
				//root.
				foreach (JToken token in root.Children()) {
					Language language = new Language();
					JProperty prop = (JProperty)token;
					JToken val = prop.Value;

					//Console.WriteLine("{0}, name: {1}", prop.Type, prop.Name); // 
					//Console.WriteLine("prop: {0} {1}  / val: {2} {3}", prop.Type, prop.HasValues, val.Type, val.HasValues); // prop: Property True  / val: Object True

					language.code = prop.Name;
					//language.names = prop.Value<string[]>("names");
					language.names = val["names"].Select(t => (string)t).ToArray();
					language.scriptCodes = val["scripts"].Select(t => (string)t).ToArray();

					language.familyCode = val.Value<string>("family"); // == val["family"]
					language.langType = val.Value<string>("type");
					language.translitModule = val.Value<string>("translit_module");

					JToken entryName = val["entry_name"];
					if (entryName != null) {
						language.entry_name_from = entryName["from"].Select(t => (string)t).ToArray();
						language.entry_name_to = entryName["to"].Select(t => (string)t).ToArray();
					}

					//Console.WriteLine("{0}: {1}, family: {2}, {3}, {4}", language.code, language.categoryName, language.familyCode, language.langType, language.entry_name_from); // 

					codeIndex[language.code] = language;
					nameIndex[language.canonicalName] = language;
					catnameIndex[language.categoryName] = language;
				}

			}
		}


	}

}

