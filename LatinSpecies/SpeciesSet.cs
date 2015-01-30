using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using LumenWorks.Framework.IO.Csv;

namespace beastie
{
	// A set of species
	public class SpeciesSet
	{
		//TODO: alternatively read the data from Catalogue of Life's MySQL database.

		const int expectedRecordCount = 2485491; //includes synonyms (2014). 1349636 for just species 2013;

		private List<Species> species;

		public SpeciesSet ()
		{
		}

		public void ReadCsv(string filename) {
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

			Console.WriteLine(string.Format("record count (1): {0}", csv.CurrentRecordIndex + 1));
			Console.WriteLine(string.Format("record count (2): {0}", species.Count));
		}

		public bool Contains(string genus, string epithet) {
			var query = new Species(genus, epithet);
			return species.Contains(query);
		}

		public StemGroups GroupEpithetStems() {
			StemGroups groups = new StemGroups();
			foreach (Species sp in species) {
				groups.AddWord(sp.epithet, sp);
				groups.AddWord(sp.genus, sp);
			}

			return groups;
		}

		public List<Species> AllSpecies() {
			return species;
		}

	}
}

