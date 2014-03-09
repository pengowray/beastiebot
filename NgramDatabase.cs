//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace beastie
{
	public class NgramDatabase : IDisposable
	{
		private MySqlConnection connection;
		private MySqlCommand addLemmaCommand;
		private MySqlParameter lemmaParam;
		private MySqlParameter corpusParam;
		private MySqlParameter matchParam;
		private MySqlParameter volumeParam;

		public NgramDatabase ()
		{
			connection = CatalogueOfLifeDatabase.Connection();
			addLemmaCommand = connection.CreateCommand();


			//TODO ERROR XXX: lemmas in database are not case sensitive!


			//TODO: should use INSERT … ON DUPLICATE KEY UPDATE syntax, except primary key needs to be lemma + corpus (instead of id)
			string query_addLemmaCount = @"
				 USE pengo;
				 INSERT INTO pengo.ng_lemmas (lemma, corpus, match_count, volume_count) 
				 SELECT * FROM (SELECT @lemma as lemma, @corpus as corpus, 0 as match_count, 0 as volume_count) AS tmp
				 WHERE NOT EXISTS (
					SELECT lemma FROM pengo.ng_lemmas 
					WHERE lemma = @lemma AND corpus = @corpus
				 ) LIMIT 1;

				UPDATE ng_lemmas
				SET match_count = match_count + @match_count, 
					volume_count = volume_count + @volume_count
				WHERE lemma = @lemma and corpus = @corpus;";
			
			connection.Open();
			addLemmaCommand.CommandText = query_addLemmaCount;
			lemmaParam = addLemmaCommand.Parameters.Add("lemma", MySqlDbType.VarBinary);
			corpusParam = addLemmaCommand.Parameters.Add("corpus", MySqlDbType.VarString);
			matchParam = addLemmaCommand.Parameters.Add("match_count", MySqlDbType.Int64);
			volumeParam = addLemmaCommand.Parameters.Add("volume_count", MySqlDbType.Int64);
			addLemmaCommand.Prepare();
		}

		public void Dispose() {
			addLemmaCommand.Dispose();
			connection.Close();
			connection.Dispose();
		}

		public void CreateTables() {
			string query = 
				@"CREATE TABLE IF NOT EXISTS pengo.ng_lemmas (
					id INT UNSIGNED NOT NULL AUTO_INCREMENT,
					lemma VARBINARY(255) NOT NULL,
					corpus VARCHAR(80) NOT NULL,
					match_count BIGINT ZEROFILL UNSIGNED NULL,
					volume_count BIGINT ZEROFILL UNSIGNED NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC),
					INDEX `lemma` (`lemma` ASC),
					INDEX `corpus` (`corpus` ASC) );

				CREATE TABLE IF NOT EXISTS pengo.ng_stems (
					id INT UNSIGNED NOT NULL AUTO_INCREMENT,
					stem VARBINARY(255) NOT NULL,
					corpus VARCHAR(80),
					stemmer VARCHAR(80),
					match_count BIGINT UNSIGNED ZEROFILL NULL,
					combined_volume_count BIGINT UNSIGNED ZEROFILL NULL,
					max_volume_count BIGINT UNSIGNED ZEROFILL NULL,
					most_common_lemma VARBINARY(255) NULL,
					lemma_match_count BIGINT UNSIGNED ZEROFILL NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC),
					INDEX `stem` (`stem` ASC),
					INDEX `corpus` (`corpus` ASC),
					INDEX `stemmer` (`stemmer` ASC) );";

			//TODO: stems -> lemmas table

			CatalogueOfLifeDatabase.CreatePengoDatabase();

			using (MySqlConnection conn = CatalogueOfLifeDatabase.Connection())
			using (MySqlCommand command = conn.CreateCommand()) {
				conn.Open();
				command.CommandText = query;
				command.ExecuteNonQuery();
			}
		}

		public void AddLemmaCounts(string lemma, string corpus, long matchCount, long volumeCount) {
			//TODO: cache for a bit in memory before writing to database

			//forgein films: the square, the act of killing, the hunt

			//creates an empty entry for the lemma/corpus if it doesn't exist, and adds match/volume counts

			lemmaParam.Value = Encoding.UTF8.GetBytes(lemma);
			corpusParam.Value = corpus;
			matchParam.Value = matchCount;
			volumeParam.Value = volumeCount;

			//int result = addLemmaCommand.ExecuteNonQuery();
			addLemmaCommand.ExecuteNonQuery();
		}

	}
}

