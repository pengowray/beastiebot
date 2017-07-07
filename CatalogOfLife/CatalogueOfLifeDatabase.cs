//------------------------------------------------------------------------------
//-- to start database:
// -- beastie.exe mysqld
// --
//  -- old way:
//	-- "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\bin\mysqld"
//	-- or 
//	-- "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\Server2Go.exe"
//	--
//	-- troubleshooting: make sure temp dir listed in "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\my.ini". e.g. mkdir "C:\Users\pengo\AppData\Local\Temp\Server2Go_11948"
//	--
//	-- workbench or API: connect to 127.0.0.1 port 7188, username: root (default)
//
//  -- to import wiktionary database files (example):
//  -- cd /d "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\bin\"
//  -- gzip -cd D:\ngrams\datasets-wiki\enwiktionary-20140328-categorylinks.sql.gz | mysql --port=7188 --user=root --database=enwiktionary
//  -- gzip -cd c:\temp\enwiktionary-20140328-*.sql.gz | mysql --port=7188 --user=root --database=enwiktionary

//-----------------------------------------------------------

using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using beastie.Wiktionary;

namespace beastie
{
	public class CatalogueOfLifeDatabase : MyDatabase {
		//private MySqlConnection connection;

		public bool dontStartMysqld = false;
		public string year = null; // CoL database year. overridden by mysqld.year
        private bool mysqldStarted = false;

		private static CatalogueOfLifeDatabase _instance;
		public static CatalogueOfLifeDatabase Instance() {
			if (_instance == null) {
				_instance = new CatalogueOfLifeDatabase();
			}
			return _instance;
		}

		public string DatabaseName() {
			return "col" + year + "ac";
		}


		public CatalogueOfLifeDatabase () {
		}

		public override MySqlConnection Connection() {
			if (!mysqldStarted && !dontStartMysqld) {
				var mysqld = new RunMysqld();
				// TODO: check if it's already running I guess?
				mysqld.StartDatabase();
				port = mysqld.port;
				year = mysqld.year; //TODO: should actually be the other way around
				//password = mysqld.password;
				mysqldStarted = true;
				mysqlBinLocation = mysqld.binLocation;
			}

			if (String.IsNullOrWhiteSpace(port)) {
				port = "7188"; // default for CatalogueOfLife
			}

			if (String.IsNullOrWhiteSpace(year)) {
				year = "2014";
			}

            string connectionString = ConnectionString();

            MySqlConnection connection = new MySqlConnection(connectionString);

			connection.Open();

            //TODO: check for errors
            //connection.ConnectionTimeout

            return connection;
		}



		public void BuildSpeciesTable() {
			// build species data from Collection of Life
			// see also: mysql-pengo-2013.txt
			
			//-- Main cleaned up list of species.  To be the basis of many other queries
			//-- It does not contain synonyms (because it doesn't find any, TODO: add synonyms list). 
			//-- Genus + epithet trimmed of spaces
			//-- Trailing commas or dots have been removed.
			//-- Genus capitalized
			//-- Virus entries removed (Epithets with spaces which contain any of the following words: virus, viroid, phage)
			//-- TODO: remove duplicates (except we'd lose the taxon id of the dupes.. so make new version without IDs and no dupes)

			// TODO: instead just use col2014ac._search_scientific table. much simpler. no joins.

			string query_CreateSpeciesViewAndTable = @"
DROP VIEW IF EXISTS beastie.view_col_species;
CREATE VIEW beastie.view_col_species AS
SELECT 
	CONCAT(UPPER(LEFT(TRIM(genus_word.name_element),1)), MID(TRIM(genus_word.name_element),2)) as genus, -- capitalize first letter of genus
	TRIM(TRIM(TRAILING ',' FROM TRIM(TRAILING '.' FROM `epithet_word`.`name_element`))) as epithet, -- trim trailing , or .
	taxon.id as taxon_id, 
	taxon_detail.scientific_name_status_id
FROM taxon 
	LEFT JOIN taxon_name_element 		AS epithet_element	ON (epithet_element.taxon_id = taxon.id)
	LEFT JOIN scientific_name_element	AS epithet_word 	ON (epithet_element.scientific_name_element_id = epithet_word.id) 
	LEFT JOIN taxon						AS genus_taxon 		ON (epithet_element.parent_id = genus_taxon.id)
	LEFT JOIN taxon_name_element		AS genus_element 	ON (genus_element.taxon_id = genus_taxon.id) 
	LEFT JOIN scientific_name_element	AS genus_word		ON (genus_element.scientific_name_element_id = genus_word.id)
	LEFT JOIN taxon_detail 	ON (taxon.id = taxon_detail.taxon_id)
WHERE 
	taxon.taxonomic_rank_id = 83
	AND genus_taxon.taxonomic_rank_id = 20 -- not strictly needed as it's always 20.
	AND (taxon_detail.scientific_name_status_id = 1 OR taxon_detail.scientific_name_status_id = 4 OR taxon_detail.scientific_name_status_id = 5) -- NOT: ambiguous syn, misapplied name
	-- taxon_detail.scientific_name_status_id = 1 (accepted), 2=ambiguous syn, 3=misapplied name, 4=provisionally accepted name, 5=synonym
	-- note: only 1 and 4 found (not 5 as that's in other tables)
	AND (LOCATE(' ', TRIM(`epithet_word`.`name_element`)) = 0 
            OR (locate('virus',  `epithet_word`.`name_element`) = 0 
            and locate('viroid', `epithet_word`.`name_element`) = 0 
            and locate('phage',  `epithet_word`.`name_element`) = 0))
ORDER BY 
	genus, epithet, taxon_id, scientific_name_status_id;
	
-- materialized view of above

DROP TABLE IF EXISTS beastie._col_species;
CREATE TABLE beastie._col_species SELECT * FROM  beastie.view_col_species LIMIT 0, 10000000;
ALTER TABLE beastie._col_species
ADD PRIMARY KEY (`taxon_id`), 
	ADD UNIQUE INDEX `taxon_id_UNIQUE` (`taxon_id` ASC);
";
		
			CreateDatabase();
			
			using (MySqlConnection connection = Connection()) {
				Console.WriteLine("Connecting to CoL database...");
				using (MySqlCommand command = connection.CreateCommand()) {
					//MySqlCommand cmd = new MySqlCommand(query, connection);
					query_CreateSpeciesViewAndTable = "USE col" + year + "ac; " + query_CreateSpeciesViewAndTable;
					command.CommandTimeout = 900; // 900 = 15 minutes. Should hopefully be done by then.
					Console.WriteLine("Creating materialized Species table. This could take a while...");
					command.CommandText = query_CreateSpeciesViewAndTable;
					int result = command.ExecuteNonQuery();
				}
			}
		}

		public Dictionary<string,string> BranchOfLife(Species species) {
			//Connection().
			//var results = from r in 

			return null;
		}
		

	}
}

