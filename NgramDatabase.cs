//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using MySql.Data.MySqlClient;

namespace beastie
{
	public class NgramDatabase
	{
		public NgramDatabase ()
		{
		}

		public void CreateTables() {
			string query = 
				@"CREATE TABLE IF NOT EXISTS `pengo`.`ng_lemmas` (
					`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
					`lemma` VARCHAR(255) NOT NULL,
					`corpus` VARCHAR(30) NOT NULL,
					`match_count` INT ZEROFILL UNSIGNED NULL,
					`volume_count` INT ZEROFILL UNSIGNED NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC) ),
					INDEX `lemma` (`lemma` ASC),
					INDEX `corpus` (`corpus` ASC);

				CREATE TABLE IF NOT EXISTS `pengo`.`ng_stems` (
					`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
					`stem` VARCHAR(255) NOT NULL,
					`corpus` VARCHAR(30),
					`stemmer` VARCHAR(20) NOT NULL,
					`match_count` INT UNSIGNED ZEROFILL NULL,
					`combined_volume_count` INT UNSIGNED ZEROFILL NULL,
					`max_volume_count` INT UNSIGNED ZEROFILL NULL,
					`most_common_lemma` VARCHAR(255) NULL,
					`lemma_match_count` INT UNSIGNED ZEROFILL NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC) ),
					INDEX `stem` (`stem` ASC),
					INDEX `corpus` (`corpus` ASC),
					INDEX `stemmer` (`stemmer` ASC);";

			//TODO: stems -> lemmas table


			CatalogueOfLifeDatabase.CreatePengoDatabase();

			using (MySqlConnection connection = CatalogueOfLifeDatabase.Connection()) 
			using (MySqlCommand command = connection.CreateCommand()) {
				connection.Open();
				command.CommandText = query;
				int result = command.ExecuteNonQuery();
			}




		}
	}
}

