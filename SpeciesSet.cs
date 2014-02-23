using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace beastie
{
	public struct Species {
		public string genus;
		public string epithet;

		public Species(string genus, string epithet) {
			this.genus = genus;
			this.epithet = epithet;
		}

		override public string ToString() {
			return string.Format("{0} {1}", genus, epithet);
		}

		public override int GetHashCode ()
		{
			//TODO: make genus/epithet read only and cache this value
			return string.Format("{0},{1}", genus, epithet).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			//TODO: compare genus and epithet?
			return base.Equals(obj);
		}
	}

	public class SpeciesSet
	{
		//TODO: alternatively read the data from Catalogue of Life's MySQL database.

		const int expectedRecordCount = 1349636;

		private string filename;

		private List<Species> species;

		public SpeciesSet (string filename)
		{
			this.filename = filename;
		}

		public void ReadCsv() {
			// open the file "data.csv" which is a CSV file with headers
			CsvReader csv = new CsvReader(new StreamReader(filename), true);

			string[] headers = csv.GetFieldHeaders();
			if (csv.FieldCount != 2) {
				//TODO: if one field, then treat as space-separated (using first space)
				throw new Exception(string.Format("SpeciesReader found wrong number of fields. Expected 2. Found: {0}", headers.Length));
			}

			//genus = new List<string>(expectedRecordCount);
			//epithet = new List<string>(expectedRecordCount);
			species = new List<Species>(expectedRecordCount);

			while (csv.ReadNextRecord())
			{
				//long i = csv.CurrentRecordIndex;
				//genus.Add(csv[0]);
				//epithet.Add(csv[1]);
				species.Add(new Species(csv[0], csv[1]));
			}

			Console.WriteLine(string.Format("record count: {0}", csv.CurrentRecordIndex + 1));
			Console.WriteLine(string.Format("record count: {0}", species.Count));
		}

		public StemGroups GroupEpithetStems() {
			StemGroups groups = new StemGroups();
			foreach (Species sp in species) {
				groups.AddWord(sp.epithet, sp);
				groups.AddWord(sp.genus, sp);
			}

			return groups;
		}

	}
}

