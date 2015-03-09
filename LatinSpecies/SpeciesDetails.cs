using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using DotNetWikiBot;

namespace beastie {

	//Note: Status order matches database. 1=accepted, etc.
	public enum Status {not_found, accepted, ambiguous_syn, misapplied_name, provisionally_accepted_name, synonym} 

	public class SpeciesDetails //TODO: rename COLTaxonLadder
	{
		//public readonly Species species;
		public Species species;
		public bool from_id = false; // if true, this species details used col_id as a key

		public string kingdom; // Plantae, Animalia, Bacteria, Fungi, Protozoa (any others?)
		public string phylum;
		public string class_;
		public string order;
		public string superfamily;
		public string family;
		public string genus;
		public string subgenus;

		public Dictionary<string, string> commonNames; // most common keys (language_iso): eng, cmn (Mandarin Chinese), (none), spa, fra, por, zlm, rus, deu, dan, ita, fin, ces, jpn... also contains numbers like 001

		public long col_id;

		public long accepted_species_id;

		public Status status;

		private bool? monotypic = null;

		public bool isAccepted {
			get {
				return status == Status.accepted;
			}
		}

		public SpeciesDetails(string binomial) {
			this.species = new Species(binomial);
		}

		public SpeciesDetails(Species species) {
			this.species = species;
		}

		public SpeciesDetails(long col_id) {
			this.col_id = col_id;
			from_id = true;
		}

		//TODO: keep track of per-genera results, so don't re-query so much.
		//TODO: also identify obsolete genus (0 results)
		//TODO: per kingdom
		public bool isMonotypic() {
			if (monotypic == null) {
				string species_count_sql = @"SELECT count(id) as count FROM " + CatalogueOfLifeDatabase.Instance().DatabaseName() + "._search_scientific " +
					"where genus = @genus " +
					"AND species <> '' " + // species
					"AND infraspecies = '' " + // exclude subspecies
					"AND (status = 1 OR status = 4) ; "; // accepted or provisionally_accepted
	
				MySqlConnection connection = CatalogueOfLifeDatabase.Instance().Connection();
				using (connection)
				using (MySqlCommand command = connection.CreateCommand()) {

					command.CommandText = species_count_sql;
					MySqlParameter genusParam = new MySqlParameter("genus", species.genus);
					command.Parameters.Add(genusParam);
									
					var data = command.ExecuteReader();

					if (!data.HasRows || !data.Read() ) {
						//monotypic = false;
						throw new Exception("Database weirdnesss error.");
					}


					long speciesCount = data.GetInt64(0); // column "count"

					if (speciesCount > 1) {
						monotypic = false; 
					} else if (speciesCount == 1) {
						monotypic = true;
					} else {
						//TODO: might be obsolete (all synonyms)
						//throw new Exception("Not found");
						monotypic = true;
						Console.Error.WriteLine("Genus not found: " + genus);
					}
				}
			}

			return (bool)monotypic;
		}

		public enum Possibilities { None, NoArticle, NoGenusArticle, BothSameArticle, SpeciesHasOwnPage }

		Possibilities possibilities;
		string WikipediaPageName;

		// check if the page exists on Wikipedia
		public bool NeedsEnWikiArticle() {
			//TODO: cache result, and save it to a file or database
			//TODO: bring back possibility checking from NeedsEnWikiArticleOld()

			var site = BeastieBot.Instance().site;

			Page speciesPage = new Page(site, species.ToString());
			//p.LoadWithMetadata();
			speciesPage.Load();
			if (speciesPage.Exists()) {
				if (speciesPage.IsRedirect()) {
					string redirTo = speciesPage.RedirectsTo();
					Console.WriteLine(species + " => " + redirTo);
					WikipediaPageName = redirTo; // TODO/WARNING: This will be in whatever format the redirect is written. E.g. may have no capitals. May include underscores or spaces.
					//speciesPage.IsDisambig(); // TODO
					//var cats = p.GetAllCategories();
				} else {
					WikipediaPageName = speciesPage.title;
				}

				return false;
			}

			return true;

		}

		// returns true if the article is missing or could be written (e.g. it's just a redirect to the genus)
		public bool NeedsEnWikiArticleOld() { // checks for "With Possibilities" poorly
			//Checks English Wikipedia to see if the wiki page of a non-monotypic species redirects to the same place as its genus

			//Such articles ought to belong to Category:Redirects to monotypic taxa
			//And their redirect should be tagged with {{R to monotypic taxon}}

			//TODO: Also should check if the monotypic species has a {{R to monotypic taxon}}, Category:Redirects to monotypic taxa

			//TODO: don't check for possibilities with synonyms (only if article exists)

			//TODO: only check genus of relevant kingdom (if searching only one kingdom)

			if (possibilities == Possibilities.None) {
				var site = BeastieBot.Instance().site;

				Page speciesPage = new Page(site, species.ToString());
				//p.LoadWithMetadata();
				speciesPage.Load();
				if (speciesPage.Exists()) {
					if (speciesPage.GetNamespace() != 0) {
						// something weird has happened.
					}
					Console.WriteLine(speciesPage.title);
					//Console.WriteLine(speciesPage.text);
					if (speciesPage.IsRedirect()) {
						string redirTo = speciesPage.RedirectsTo();
						Console.WriteLine(species + " => " + redirTo);
						WikipediaPageName = redirTo;
						//speciesPage.IsDisambig(); // TODO
						//var cats = p.GetAllCategories();
					} else {
						WikipediaPageName = speciesPage.title;
					}
					//p.ResolveRedirect();
					//p.GetAllCategories();

					//TODO: also check species.genus + " (plant)" (if kindom: plantae)

					//TODO: use a PageList instead of making so many separate requessts         
					//PageList pl = new PageList(site);

					Page genusPage = new Page(site, species.genus + " (genus)");
					genusPage.Load();
					if (!genusPage.Exists()) {
						genusPage = new Page(site, species.genus);
						genusPage.Load();
					}
					genusPage.ResolveRedirect();

					if (!genusPage.Exists()) {
						possibilities = Possibilities.NoGenusArticle;
					} else {
						if (genusPage.title == speciesPage.title) {
							possibilities = Possibilities.BothSameArticle;
						} else {
							possibilities = Possibilities.SpeciesHasOwnPage;
						}
					}

				} else {
					//Console.WriteLine("Page not found.");
					possibilities = Possibilities.NoArticle;
				}
			}
			//var speciesPage = wiki.Query.allpages().Where(p => p.ns == 0 && p.filterredir// querypage();
			//var genusPage = wiki.Query.querypage(species.genus);
			//var genusPage2 = wiki.Query.querypage(species.genus + "_(genus)");

			//foreach (querypageSelect p in speciesPage.ToEnumerable()) {
			//	var ns = p.ns;
				//if (ns != 0)
			//}

			if (possibilities == Possibilities.NoArticle) return true;
			if (possibilities == Possibilities.SpeciesHasOwnPage) return false;
			if (possibilities == Possibilities.NoGenusArticle) return false;
			if (possibilities == Possibilities.BothSameArticle) {
				if (isMonotypic())
					return false;
				else
					return true;
			}
				
			return true; // error?
		}


		public void Load() {

			MySqlConnection connection = CatalogueOfLifeDatabase.Instance().Connection();

			//string sql = @"SELECT * FROM col2014ac._search_scientific where genus = @genus and species = @species and infraspecies like """"; ";
			string search_sci_sql = @"SELECT * FROM " + CatalogueOfLifeDatabase.Instance().DatabaseName() + "._search_scientific where genus = @genus and species = @species AND infraspecies = '' ORDER BY status ; ";
			string search_id_sql = @"SELECT * FROM " + CatalogueOfLifeDatabase.Instance().DatabaseName() + "._search_scientific where id = @id ; "; 

			//-- status (scientific_name_status_id): 1=accepted, 2=ambiguous syn, 3=misapplied name, 4=provisionally accepted name, 5=synonym

			//status counts  -- select status, count(id) from _search_scientific group by status order by status -- 2014
			//0=151568, 1=1,569,672, 2=4424, 3=8383, 4=154,643, 5=1,264,753
			//% of 1-5: 1=52.29%, 2=0.15%, 3=0.28%, 4=5.15%, 5=42.13%


			using (connection)
			using (MySqlCommand command = connection.CreateCommand()) {
				if (col_id != 0) {
					from_id = true;
					command.CommandText = search_id_sql;
					MySqlParameter idParam = new MySqlParameter("id", this.col_id);
					command.Parameters.Add(idParam);

				} else {
					command.CommandText = search_sci_sql;
					//MySqlParameter genusParam = command.Parameters.Add("@genus", MySqlDbType.VarChar); // MySqlDbType.VarString
					//MySqlParameter epithetParam = command.Parameters.Add("@species", MySqlDbType.VarChar);
					MySqlParameter genusParam = new MySqlParameter("genus", species.genus);
					MySqlParameter epithetParam = new MySqlParameter("species", species.epithet);
					command.Parameters.Add(genusParam);
					command.Parameters.Add(epithetParam);
				}
				//command.Prepare();

				//genusParam.Value = Encoding.UTF8.GetBytes(species.genus);
				//epithetParam.Value = Encoding.UTF8.GetBytes(species.epithet);

				//genusParam.Value = species.genus;
				//epithetParam.Value = species.epithet;

				var data = command.ExecuteReader();

				if (!data.HasRows) {
					if (from_id) {
						Console.WriteLine("failed to find: id={1}", col_id);
					} else {
						Console.WriteLine("failed to find: {0}", species);
					}
				}

				// id, kingdom, phylum, class, order, superfamily, family, genus, subgenus, species, infraspecific_marker, infraspecies, author, status, accepted_species_id, accepted_species_name, accepted_species_author, source_database_id, source_database_name
				// 1156, Animalia, Mollusca, Gastropoda, Stylommatophora, Helicoidea, Camaenidae, Noctepuna, , cerea, , , (Hedley, 1894), 1, 0, , , 1, AFD (Pulmonata)

				while (data.Read()) {
					if (data.GetString("infraspecies") != "") {
						// Subspecies

						//TODO: ignore for now, but should add to a list.. 
						continue; 
					}

					//Console.WriteLine("{0} {1} status:{2}", data["genus"], data["species"], status);

					//int int_status = data.GetByte("status"); // tinyint(1) range: -128...+127. (1 is for display)
					status = (Status)data.GetByte("status");
					//Console.WriteLine("status: {0} {1}", status, int_status);

					kingdom = (string)data["kingdom"];
					phylum = (string)data["phylum"];
					order = (string)data["order"];
					class_ = (string)data["class"];
					family = (string)data["family"];

					if (from_id) {
						species = new Species((string)data["genus"], (string)data["species"]);
					} else {
						col_id = data.GetInt64("id"); // INT(10)
					}

					if (status == Status.accepted) { // accepted name
						//TODO: other fields

						//TODO: common names

						//if (kingdom == "Animalia" && phylum == "Chordata") {
							//Console.WriteLine("{0} ({1}, {2})", species, kingdom, phylum);
							//Console.WriteLine("# [[{0}]]", species);
						//}

					} else { // if (status == 5) { // synonym
						//TODO
						//accepted_species_id

						accepted_species_id = data.GetInt64("accepted_species_id");
					}

					break; //TODO: read others and gather other synonyms (and subspecies?)
				}

				data.Close();
			}

			if (isAccepted) {
				QueryCommonName();
			}
		}

		public SpeciesDetails AcceptedSpeciesDetails() {
			if (accepted_species_id != 0) {
				return new SpeciesDetails(accepted_species_id);
			} else {
				// error
				return null;
			}
		}

		private void QueryCommonName() {

			MySqlConnection  connection = CatalogueOfLifeDatabase.Instance().Connection();

			string common_name_sql = 
			@"USE " +  CatalogueOfLifeDatabase.Instance().DatabaseName() + @";
				SELECT _search_scientific.id, language_iso, country_iso, name, transliteration, free_text
				FROM _search_scientific
				JOIN common_name ON common_name.taxon_id = _search_scientific.id
				JOIN common_name_element ON common_name.common_name_element_id = common_name_element.id
				LEFT JOIN region_free_text ON common_name.region_free_text_id = region_free_text.id
				WHERE _search_scientific.id = @id;";


			using (connection)
			using (MySqlCommand command = connection.CreateCommand()) {

				command.CommandText = common_name_sql;
				//MySqlParameter genusParam = command.Parameters.Add("@genus", MySqlDbType.VarChar); // MySqlDbType.VarString
				//MySqlParameter epithetParam = command.Parameters.Add("@species", MySqlDbType.VarChar);
				MySqlParameter idParam = new MySqlParameter("id", col_id);
				command.Parameters.Add(idParam);

				//command.Prepare();
				//idParam.Value = col_id;

				MySqlDataReader data = command.ExecuteReader();
				while (data.Read()) {
					if (commonNames == null) {
						commonNames = new Dictionary<string, string>();
					}

					string lang = data.GetStringSafe("language_iso", "(none)").ToLowerInvariant().Trim();
					string name = data.GetString("name");

					if (string.IsNullOrEmpty(lang)) {
						lang = "(none)";
					}

					commonNames[lang] = name;
				}
			}

		}

		public string MostEnglishName() {
			if (commonNames == null)
				return null;

			if (commonNames.Keys.Count == 0) {
				return null;
			}

			if (commonNames.ContainsKey("eng")) {
				return commonNames["eng"];
			} 

			if (commonNames.ContainsKey("003")) { // Appears to be English, e.g. for Pinus sylvestris
				return commonNames["003"];
			} 

			if (commonNames.ContainsKey("(none)")) {
				return commonNames["(none)"];
			}
				
			return commonNames[commonNames.Keys.First()];
		}

		public string PrettyKingdomPhylum() {
			if (string.IsNullOrEmpty(kingdom)) {
				return "";
			}

			if (string.IsNullOrEmpty(phylum)) {
				return string.Format("({0})", kingdom);
			} 

			return string.Format("({0}, {1})", phylum, kingdom);

		}

		public string PrettyPhylumClass() {
			if (kingdom == "Plantae") {
				if (string.IsNullOrEmpty(order)) {
					return "";
				}

				if (string.IsNullOrEmpty(family)) {
					return string.Format("({0})", order);
				}

				return string.Format("({0}, {1})", family, order);

			} else {

				if (string.IsNullOrEmpty(phylum)) {
					return "";
				}

				if (string.IsNullOrEmpty(class_)) {
					return string.Format("({0})", phylum);
				}

				return string.Format("({0}, {1})", class_, phylum);
			}
		}

		public string PrettyOrderFamily() {

			if (string.IsNullOrEmpty(order)) {
				return "";
			}

			if (string.IsNullOrEmpty(family)) {
				return string.Format("({0})", order);
			}

			return string.Format("({0}, {1})", family, order);
		}

	}
}

