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

		private MySqlConnection addIndexConnection;
		private MySqlCommand addIndexCommand;
		private MySqlParameter indexStemParam;
		private MySqlParameter indexLemmaParam;
		private MySqlParameter indexStemmerParam;
		private MySqlParameter indexCorpusParam;
		private MySqlParameter indexArchetypicalnessParam;

		public NgramDatabase ()
		{

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

			var coldb = new CatalogueOfLifeDatabase();
			connection = coldb.Connection();

			addLemmaCommand = connection.CreateCommand();
			addLemmaCommand.CommandText = query_addLemmaCount;
			lemmaParam = addLemmaCommand.Parameters.Add("lemma", MySqlDbType.VarBinary);
			corpusParam = addLemmaCommand.Parameters.Add("corpus", MySqlDbType.VarString);
			matchParam = addLemmaCommand.Parameters.Add("match_count", MySqlDbType.Int64);
			volumeParam = addLemmaCommand.Parameters.Add("volume_count", MySqlDbType.Int64);
			addLemmaCommand.Prepare();

			string query_addStemIndex = @"
				USE pengo;
				INSERT INTO pengo.stem_index (stem, lemma, stemmer, corpus, archetypicalness) 
				VALUES (@stem, @lemma, @stemmer, @corpus, @archetypicalness);";

			addIndexConnection = coldb.Connection();

			addIndexCommand = connection.CreateCommand();
			addIndexCommand.CommandText = query_addStemIndex;
			indexStemParam = addIndexCommand.Parameters.Add("stem", MySqlDbType.VarBinary);
			indexLemmaParam = addIndexCommand.Parameters.Add("lemma", MySqlDbType.VarBinary);
			indexStemmerParam = addIndexCommand.Parameters.Add("stemmer", MySqlDbType.VarString);
			indexCorpusParam = addIndexCommand.Parameters.Add("corpus", MySqlDbType.VarString);
			indexArchetypicalnessParam = addIndexCommand.Parameters.Add("archetypicalness", MySqlDbType.Int64);
			addIndexCommand.Prepare();
		}

		public void Dispose() {
			addLemmaCommand.Dispose();
			addIndexCommand.Dispose();
			connection.Close();
			connection.Dispose();
		}

		public void CreateTables() {
			string query = 
				@"CREATE TABLE IF NOT EXISTS pengo.ng_lemmas (
					id INT UNSIGNED NOT NULL AUTO_INCREMENT,
					lemma VARBINARY(255) NOT NULL,
					corpus VARCHAR(80) NOT NULL,
					match_count BIGINT UNSIGNED NULL,
					volume_count BIGINT UNSIGNED NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC),
					INDEX `lemma` (`lemma` ASC),
					INDEX `corpus` (`corpus` ASC) );

				CREATE TABLE IF NOT EXISTS pengo.stem_index (
					id INT UNSIGNED NOT NULL AUTO_INCREMENT,
					stem VARBINARY(255) NOT NULL,
					lemma VARBINARY(255) NOT NULL,
					stemmer VARCHAR(80) NOT NULL,
					corpus VARCHAR(80) NOT NULL,
					archetypicalness BIGINT, -- e.g. match_count for ngrams, fewest transformations for stems, closeness to a lemma for wiktionary
					PRIMARY KEY (`id`),
					INDEX `stem` (`stem` ASC),
					INDEX `lemma` (`lemma` ASC),
					INDEX `corpus` (`corpus` ASC),
					INDEX `stemmer` (`stemmer` ASC) );

				-- replaces ng_stems
				CREATE TABLE IF NOT EXISTS pengo.ng_stem_stats (
					id INT UNSIGNED NOT NULL AUTO_INCREMENT,
					stem VARBINARY(255) NOT NULL,
					corpus VARCHAR(80),
					stemmer VARCHAR(80),
					match_count BIGINT UNSIGNED NULL,
					combined_volume_count BIGINT UNSIGNED NULL,
					max_volume_count BIGINT UNSIGNED NULL,
					most_common_lemma VARBINARY(255) NULL,
					lemma_match_count BIGINT UNSIGNED NULL,
					PRIMARY KEY (`id`),
					UNIQUE INDEX `id_UNIQUE` (`id` ASC),
					INDEX `stem` (`stem` ASC),
					INDEX `corpus` (`corpus` ASC),
					INDEX `stemmer` (`stemmer` ASC) );";

			//TODO: stems -> lemmas table

			CatalogueOfLifeDatabase.Instance().CreateBeastieDatabase();

			using (MySqlConnection conn = CatalogueOfLifeDatabase.Instance().Connection())
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

		public void AddIndex(string stem, string lemma, string stemmer, string corpus, long archetypicalness) {
			//todo: delete old entries

			indexStemParam.Value = Encoding.UTF8.GetBytes(stem);
			indexLemmaParam.Value = Encoding.UTF8.GetBytes(lemma);
			indexStemmerParam.Value = stemmer;
			indexCorpusParam.Value = corpus;
			indexArchetypicalnessParam.Value = archetypicalness;

			addIndexCommand.ExecuteNonQuery();
		}

		public void CreateScannoIndexNgram(string corpus = "eng-fiction-all-1950+") {
			string stemmer = "scanno";
			
			string query_allWiktionaryWords = @"SELECT lemma, volume_count, match_count FROM pengo.ng_lemmas;";
			
			using (MySqlConnection conn = CatalogueOfLifeDatabase.Instance().Connection()) {
				conn.Open();
				MySqlCommand list = conn.CreateCommand();
				list.CommandText = query_allWiktionaryWords;
				
				using (MySqlDataReader dataReader = list.ExecuteReader()) {
					
					//Read the data and store them in the list
					while (dataReader.Read())
					{
						byte[] rawLemma = (byte[]) dataReader["lemma"];
						Lemma lemma = new Lemma(rawLemma, false);
						string stem = lemma.ScannoInsensitiveNormalized();

						if (stem != null && stem != "") {
							long archiness = dataReader.GetInt64("volume_count"); // volumes the exact lemma appears in (vs match_count which includes similar (cleaned) matches)
							AddIndex (stem, lemma.raw, stemmer, corpus, archiness);
						}
					}
				}
			}

		}

		public void CreateScannoIndexWiktionary() {
			string corpus = "enwikt"; //todo: date too?
			string stemmer = "scanno";

			string query_allWiktionaryWords = @"SELECT distinct lemma FROM pengo.wikt_lemmas_mat;";

			using (MySqlConnection conn = CatalogueOfLifeDatabase.Instance().Connection()) {
				conn.Open();
				MySqlCommand list = conn.CreateCommand();
				list.CommandText = query_allWiktionaryWords;

				using (MySqlDataReader dataReader = list.ExecuteReader()) {
				
					//Read the data and store them in the list
					while (dataReader.Read())
					{

						byte[] rawLemma = (byte[]) dataReader[0];
						Lemma lemma = new Lemma(rawLemma, true);

						string stem = lemma.ScannoInsensitiveNormalized();
						if (stem != null && stem != "") {
							long archiness = 1; // todo
							AddIndex (stem, lemma.raw, stemmer, corpus, archiness);
						}
					}
				}
			}
		}
	}
}

