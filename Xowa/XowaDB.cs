using System;
using System.Data;
using System.Data.SQLite;
//using SQLite;
using System.Text;

namespace beastie {
	public class XowaDB
	{

		private SQLiteConnection sql_con_text;
		private SQLiteConnection sql_con_page;

		private SQLiteCommand sql_cmd;
		private SQLiteDataAdapter DB;
		private DataSet DS = new DataSet();
		private DataTable DT = new DataTable();

		public XowaDB() {

		}

		private void SetConnection()  { 
			//string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v1.10.1.1\wiki\en.wiktionary.org\";
			string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v2.1.1.1\wiki\en.wiktionary.org\";
			string pageDbFile = @"en.wiktionary.org.000.sqlite3";
			string textDbFile = @"en.wiktionary.org.002.sqlite3"; // TODO: actually, 002 should be coming from page.page_file_id i think

			sql_con_text = new SQLiteConnection
				("Data Source=" + dir + textDbFile + ";Version=3;New=False;Compress=True;"); 

			sql_con_page = new SQLiteConnection
				("Data Source=" + dir + pageDbFile + ";Version=3;New=False;Compress=True;"); 
		}

		public WiktionaryEntry ReadEntry(string pageName) {
			long id = FindPageID(pageName);
			if (id == 0)
				return null;

			string text = ReadPageText(id);
			if (text == null)
				return null;

			var entry = new WiktionaryEntry();
			entry.id = id;
			entry.text = text;

			return entry;
		}

		public string ReadPage(string pageName) {
			long id = FindPageID(pageName);
			string text = ReadPageText(id);

			return text;
		}

		public long FindPageID(string page) { 
			page = page.Replace(" ", "_");
			//TODO: any other escaping needed?

			//TODO: add page_namespace paramater

			//TODO: capture "is redirect" field

			string sql = "SELECT page_id FROM page WHERE page_title = @page_title AND page_namespace = 0 ;";

			SetConnection(); 
			using (sql_con_page) {
				sql_con_page.Open(); 
				sql_cmd = sql_con_page.CreateCommand(); 
				sql_cmd.CommandText = sql; 
				sql_cmd.Parameters.Add("@page_title", DbType.String).Value = page;
				SQLiteDataReader reader = sql_cmd.ExecuteReader(); 

				bool success = reader.Read();
				if (success) {
					long page_id = reader.GetInt64(0);
					//old_text = GzipReader.Decompress(old_text);
					//string text = Encoding.UTF8.GetString(old_text);

					return page_id;
				}
			}

			// throw something
			return 0;
		}

		public string ReadPageText(long page_id=745000) {

			//string table = "text";

			// TODO: parameterize

			string sql = "SELECT old_text FROM text WHERE page_id = @page_id ;";

			//read field old_text and un-gzip it

			SetConnection(); 
			using (sql_con_text) {
				sql_con_text.Open(); 
				sql_cmd = sql_con_text.CreateCommand(); 
				sql_cmd.CommandText = sql; 
				sql_cmd.Parameters.Add("@page_id", DbType.Int64).Value = page_id;
				SQLiteDataReader reader = sql_cmd.ExecuteReader(); 

				bool success = reader.Read();
				if (success) {
					byte[] old_text = reader.GetBytes("old_text");
					old_text = GzipReader.Decompress(old_text);
					string text = Encoding.UTF8.GetString(old_text);

					return text;
				}
			}
			//sql_con.Close();

			return "";
			//TODO: throw something
		}
	}
}

