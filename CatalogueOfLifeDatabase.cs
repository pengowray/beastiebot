//------------------------------------------------------------------------------
//-- to start database:
//	-- "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\bin\mysqld"
//	-- or 
//	-- "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\Server2Go.exe"
//	--
//	-- troubleshooting: make sure temp dir listed in "D:\Program Files (x86)\Catalogue of Life\2013 Annual Checklist\server\mysql\my.ini". e.g. mkdir "C:\Users\pengo\AppData\Local\Temp\Server2Go_11948"
//	--
//	-- workbench or API: connect to 127.0.0.1 port 7188, username: root (default)
//------------------------------------------------------------------------------
using System;
using MySql.Data.MySqlClient;

namespace beastie
{
	public class CatalogueOfLifeDatabase
	{
		//private MySqlConnection connection;

		private const string query_CreateDatabasePengo = 
			@"CREATE DATABASE IF NOT EXISTS pengo 
				CHARACTER SET utf8 
				DEFAULT COLLATE utf8_general_ci;";

		public CatalogueOfLifeDatabase ()
		{
		}

		public static MySqlConnection Connection() {
			string server = "localhost";
			string port = "7188";
			string database = "";
			string uid = "root";
			string password = "";
			string connectionString = "SERVER=" + server + ";" + "PORT=" + port + ";" + "DATABASE=" + 
				database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
			
			MySqlConnection connection = new MySqlConnection(connectionString);
			//connection.ConnectionTimeout
			return connection;
		}

		public static void CreatePengoDatabase() {
			using (MySqlConnection connection = Connection())
			using (MySqlCommand command = connection.CreateCommand()) {
				connection.Open();

				command.CommandText = query_CreateDatabasePengo;
				Console.WriteLine("Checking for / creating Pengo database...");
				int result = command.ExecuteNonQuery();
			}


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


			string query_CreateSpeciesViewAndTable = @"
USE col2013ac;
DROP VIEW IF EXISTS pengo.col_species_view;
CREATE VIEW pengo.col_species_view AS
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

DROP TABLE IF EXISTS pengo.col_species_mat;
CREATE TABLE pengo.col_species_mat SELECT * FROM  pengo.col_species_view LIMIT 0, 10000000;
ALTER TABLE pengo.col_species_mat
ADD PRIMARY KEY (`taxon_id`), 
	ADD UNIQUE INDEX `taxon_id_UNIQUE` (`taxon_id` ASC);
";
		
			CreatePengoDatabase();
			
			using (MySqlConnection connection = Connection()) {
				Console.WriteLine("Connecting to CoL database...");
				connection.Open();
				using (MySqlCommand command = connection.CreateCommand()) {
					//MySqlCommand cmd = new MySqlCommand(query, connection);
					command.CommandTimeout = 900; // 900 = 15 minutes. Should hopefully be done by then.
					Console.WriteLine("Creating materialized Species table. This could take a while...");
					command.CommandText = query_CreateSpeciesViewAndTable;
					int result = command.ExecuteNonQuery();
				}
			}

		}


		

	}
}

