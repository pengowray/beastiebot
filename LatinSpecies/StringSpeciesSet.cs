using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LumenWorks.Framework.IO.Csv;

namespace beastie
{

	// A set of species, which are stored just as strings
	public class StringSpeciesSet
	{
		//TODO: alternatively read the data from Catalogue of Life's MySQL database.

		private HashSet<string> species;

		public void ReadCsv(string filename) {
			// open the file "data.csv" which is a CSV file with headers
			//CsvReader csv = new CsvReader(new StreamReader(filename), true);
			//string[] headers = csv.GetFieldHeaders();

			//using( var reader = new CsvReader( new StreamReader(filename, Encoding.UTF8) )) {
			using (	CsvReader csv = new CsvReader(new StreamReader(filename, Encoding.UTF8), true)) {

				//string[] headers = reader.FieldHeaders;
				string[] headers = csv.GetFieldHeaders();
				//if (csv.FieldCount != 2)
				if (headers.Length != 2) {
					//TODO: if one field, then treat as space-separated (using first space)
					throw new Exception(string.Format("SpeciesReader found wrong number of fields. Expected 2. Found: {0}", headers.Length));
				}

				//genus = new List<string>(expectedRecordCount);
				//epithet = new List<string>(expectedRecordCount);
				species = new HashSet<string>();

				while (csv.ReadNextRecord())
				{
					//long i = csv.CurrentRecordIndex;
					//genus.Add(csv[0]);
					//epithet.Add(csv[1]);
					species.Add(string.Format("{0} {1}", csv[0], csv[1]));
				}

				Console.WriteLine(string.Format("species set: record count (1): {0}", csv.CurrentRecordIndex + 1));
				Console.WriteLine(string.Format("species set: record count (2): {0}", species.Count));
			}
		}

		public bool Contains(string species) {
			return species.Contains(species);
		}

		public bool Contains(string genus, string epithet) {
			return species.Contains(string.Format("{0} {1}",genus, epithet));
		}

		/*
		public StemGroups GroupEpithetStems() {
			StemGroups groups = new StemGroups();
			foreach (Species sp in species) {
				groups.AddWord(sp.epithet, sp);
				groups.AddWord(sp.genus, sp);
			}

			return groups;
		}
		*/

		public string AllFirstChars() {
			var chars = new HashSet<char>();
			foreach (string sp in species) {
				chars.Add(sp[0]);
			}
			var charList = chars.ToList();
			charList.Sort();
			return new string(charList.ToArray());
		}

		public string AllOtherChars() {
			var chars = new HashSet<char>();
			foreach (string sp in species) {
				bool isFirst = true;
				foreach (char spOther in sp.ToCharArray()) {
					if (isFirst) {
						isFirst = false;
					} else {
						chars.Add(spOther);
					}
				}
			}
			var charList = chars.ToList();
			charList.Sort();
			return new string(charList.ToArray());

		}

	}
}

