using System;
using System.Data;
using System.Data.SQLite;
//using SQLite;
using System.Text;

namespace beastie {
	public class XowaDB
	{

		private SQLiteConnection sql_con;
		private SQLiteCommand sql_cmd;
		private SQLiteDataAdapter DB;
		private DataSet DS = new DataSet();
		private DataTable DT = new DataTable();

		public XowaDB() {

		}

		private void SetConnection()  { 
			string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v1.10.1.1\wiki\en.wiktionary.org\";
			string dbFile = @"en.wiktionary.org.002.sqlite3";

			sql_con = new SQLiteConnection
				("Data Source=" + dir + dbFile + ";Version=3;New=False;Compress=True;"); 
		}

		public void ReadPageText(long page_id=745000) {

			//string table = "text";

			// TODO: parameterize

			string sql = "SELECT old_text FROM text WHERE page_id = @page_id ;";

			//read field old_text and un-gzip it

			SetConnection(); 
			sql_con.Open(); 
			sql_cmd = sql_con.CreateCommand(); 
			sql_cmd.CommandText = sql; 
			sql_cmd.Parameters.Add("@page_id", DbType.Int64).Value = page_id;
			SQLiteDataReader reader = sql_cmd.ExecuteReader(); 

			bool success = reader.Read();
			if (success) {
				byte[] old_text = reader.GetBytes("old_text");
				old_text = GzipReader.Decompress(old_text);
				string text = Encoding.UTF8.GetString(old_text);

				Console.WriteLine(text);
			}

			sql_con.Close();
		}
	}
}

