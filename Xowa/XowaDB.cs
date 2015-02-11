using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
//using SQLite;
using System.Text;


namespace beastie {
	/*
	class XowaWikiParams {
		public string basedir;
		public string path;
		public string defaultIndexDb = "000";
		public string defaultTextDb = "002";
	}
	*/
	public class XowaDB
	{
		//public string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v1.10.1.1\";
		public string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v2.1.1.1\";
		public string path = @"wiki\{0}\{0}.{1}.sqlite3"; // 0 = site ("en.wikipedia.org"), 1 = "000" or "002"

		public bool uppercaseFirstLetter;

		public string site; // e.g. "en.wiktionary.org"

		private string page_table_file_index = "000";
		public string text_table_file_index = "002"; // default text table database
		private Dictionary<string, SQLiteConnection> connections = new Dictionary<string, SQLiteConnection>(); // "000" => connection

		private SQLiteCommand sql_cmd;
		//private SQLiteDataAdapter DB;
		private DataSet DS = new DataSet();
		private DataTable DT = new DataTable();

		//public static XowaWikiParams enwikt;
		//public static XowaWikiParams enwiki;

		static XowaDB() {
			/*
			enwikt = new XowaWikiParams();
			enwikt.basedir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v2.1.1.1\";
			enwikt.path = @"wiki\en.wiktionary.org\en.wiktionary.org.{0}.sqlite3";
			enwikt.defaultIndexDb = "000";
			enwikt.defaultTextDb = "002";

			enwiki = new XowaWikiParams();
			enwiki.basedir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v2.1.1.1\";
			enwikt.path = @"wiki\en.wikipedia.org\en.wikipedia.org.{0}.sqlite3";
			enwiki.defaultIndexDb = "000";
			enwiki.defaultTextDb = "002";
			*/
		}

		public XowaDB(string site = "en.wikipedia.org") {
			this.site = site;
		}

		SQLiteConnection GetConnection(string db = "000") {
			if (!connections.ContainsKey(db)) {
				string datasrc = dir + string.Format(path, site, db);
				var conn = new SQLiteConnection("Data Source=" + datasrc + ";Version=3;New=False;Compress=True;"); 
				//Console.WriteLine("connection path: " + datasrc);
				//Console.Out.Flush();
				connections[db] = conn;
				conn.Open();
			}

			var c = connections[db];
			return c;
		}

		SQLiteConnection GetConnection(int db = 0) {
			return GetConnection(string.Format("{0:000}", db));
		}


		private void SetConnection()  { 
			//string dir = @"D:\ngrams\datasets-xowa\xowa_app_windows_64_v2.1.1.1\wiki\en.wiktionary.org\";
			//string pageDbFile = @"en.wiktionary.org.000.sqlite3";
			//string textDbFile = @"en.wiktionary.org.002.sqlite3"; // TODO: actually, 002 should be coming from page.page_file_id i think

			//sql_con_page = new SQLiteConnection ("Data Source=" + dir + pageDbFile + ";Version=3;New=False;Compress=True;"); 

			//sql_con_text = new SQLiteConnection("Data Source=" + dir + textDbFile + ";Version=3;New=False;Compress=True;"); 

		}


		public XowaPage ReadXowaPage(string pageName) {
			return ReadPage(pageName);
		}

		//TODO: merge WiktionaryEntry with XowaPage
		public WiktionaryEntry ReadWiktionaryEntry(string pageName) {
			var page = ReadPage(pageName);
			var entry = new WiktionaryEntry();
			entry.id = page.pageId;
			entry.text = page.text;

			return entry;
		}

		public XowaPage ReadPage(string page) { 
			page = page.Trim();
			page = page.Replace(' ', '_');
			if (uppercaseFirstLetter) {
				page = page.UpperCaseFirstChar();
			}

			//TODO: any other escaping needed?
			//TODO: add page_namespace paramater
			//TODO: capture "is redirect" field

			string sql = "SELECT page_id, page_title, page_file_idx, page_is_redirect, page_len FROM page WHERE page_title = @page_title AND page_namespace = 0 ;";
			//SELECT page_id, page_file_idx, page_touched, page_is_redirect, page_len, 

			//TODO: get date from page_touched varchar(14), e.g. "20141228201521"
			//TODO: wiki date? in db 000: table "xowa_cfg": "wiki.init"	"props.modified_latest"	"2015-01-02 16:58:33"

			//SetConnection(); 
			var conn = GetConnection(page_table_file_index);
			//using () {

			//conn.Open();
			sql_cmd = conn.CreateCommand(); 
			sql_cmd.CommandText = sql; 
			sql_cmd.Parameters.Add("@page_title", DbType.String).Value = page;
			SQLiteDataReader reader = sql_cmd.ExecuteReader(); 

			bool success = reader.Read();
			if (success) {
				long page_id = reader.GetInt64(0); // int(10) unsigned
				string title = reader.GetString(1);
				int page_file_idx = reader.GetInt32(2); // "integer"
				int page_is_redirect = reader.GetInt16(3); 
				int expected_len = reader.GetInt32(4); // page_len
				//old_text = GzipReader.Decompress(old_text);
				//string text = Encoding.UTF8.GetString(old_text);
				if (page_id != 0) {
					var entry = new XowaPage();
					entry.pageId = page_id + "";
					entry.text = ReadPageText(page_id, page_file_idx, expected_len);
					entry.title = title.Replace('_', ' ');
					entry.xowa_redirect = (page_is_redirect == 1);
					entry.siteDomain = "https://" + site;

					//Console.WriteLine("page_id=" + page_id + " page_file_idx=" + page_file_idx + " title=" + title);

					return entry;
				}
			}
			//conn.Close();
			//}

			return null;

			// throw something
			//return 0;
		}

		//public string ReadPageText(long page_id=745000, int page_file_idx = 2) {

		public string ReadPageText(long page_id=745000, int page_file_idx = 2, int expected_len = -1) {

			//Console.WriteLine("page_id=" + page_id + " page_file_idx=" + page_file_idx);
			//string table = "text";

			// TODO: parameterize

			string sql = "SELECT old_text FROM text WHERE page_id = @page_id ;";

			//read field old_text and un-gzip it

			//SetConnection();
			var conn = GetConnection(page_file_idx);
			//var conn = GetConnection(text_table_file_index); // page_file_idx is for what?
			//conn.Open(); 
			sql_cmd = conn.CreateCommand(); 
			sql_cmd.CommandText = sql; 
			sql_cmd.Parameters.Add("@page_id", DbType.Int64).Value = page_id;
			SQLiteDataReader reader = sql_cmd.ExecuteReader(); 

			bool success = reader.Read();
			if (success) {
				byte[] old_text = reader.GetBytes("old_text");
				byte[] uncompressed_text = GzipReader.Decompress(old_text);
				string text = Encoding.UTF8.GetString(uncompressed_text);

				if (old_text.Length > 0 && uncompressed_text.Length == 0) {
					Console.Error.WriteLine(page_id + " did not decompress: " + old_text);
				}
				if (old_text.Length != expected_len) {
					Console.Error.WriteLine(page_id + " WRONG length. expected=" + expected_len + " actual=" + old_text.Length);
				}
				//Console.Error.WriteLine("text OK: " + page_file_idx);
				return text;
			} else {
				//Console.Error.WriteLine("text NOT FOUND: " + page_file_idx);
			}
			//conn.Close();

			return null; // string.Empty;
			//TODO: throw something
		}
	}
}

