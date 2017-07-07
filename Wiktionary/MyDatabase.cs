using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace beastie.Wiktionary {
    public class MyDatabase {
        public string port = "3306";
        public string password = ""; // mysql password. TODO: command line parameter to override blank password
        public string server = "127.0.0.1"; // "localhost"; "10.0.75.1"
                                            //string port = "7188"; // or 3306, or 7189
        public string database = ""; // default database. have generally left blank and used "USE mydatabase;" in sql.
        public string uid = "root";

        public string mysqlBinLocation = ""; // to use RunMySqlImport()

        public virtual string ConnectionString() {
            string connectionString =
                "SERVER=" + server + ";" +
                "PORT=" + port + ";" +
                "DATABASE=" + database + ";" +
                "UID=" + uid + ";" +
                "PASSWORD=" + password + ";";
            return connectionString;
        }

        public virtual MySqlConnection Connection() {

            if (String.IsNullOrWhiteSpace(port)) {
                port = "3306"; // default for MySQL
            }

            string connectionString = ConnectionString();

            MySqlConnection connection = new MySqlConnection(connectionString);

            connection.Open();

            //TODO: check for errors
            //connection.ConnectionTimeout

            return connection;
        }


        public void CreateDatabase(string name="beastie") {
            string query_CreateDatabaseBeastie =
                String.Format(@"CREATE DATABASE IF NOT EXISTS {0} CHARACTER SET utf8 DEFAULT COLLATE utf8_general_ci;", name);

            using (MySqlConnection connection = Connection())
            using (MySqlCommand command = connection.CreateCommand()) {

                command.CommandText = query_CreateDatabaseBeastie;
                Console.WriteLine("Checking for / creating {0} database...", name);
                int result = command.ExecuteNonQuery();
                Console.WriteLine("OK");
            }
        }

        public void CreateWikiDatabase(string name="enwiktionary") {
            string query_CreateAndUseDatabaseWiktionary =
                String.Format(@"CREATE DATABASE IF NOT EXISTS {0} CHARACTER SET binary; USE {0};", name);

            using (MySqlConnection connection = Connection())
            using (MySqlCommand command = connection.CreateCommand()) {

                command.CommandText = query_CreateAndUseDatabaseWiktionary;
                Console.WriteLine("Checking for / creating {0} database...", name);
                int result = command.ExecuteNonQuery();
                Console.WriteLine("OK");
            }
        }

        public void ExecuteNonQuery(string commandTextSQL, string logText = null, int commandTimeout = 900) {
            using (MySqlConnection connection = Connection())
            using (MySqlCommand command = connection.CreateCommand()) {
                if (!string.IsNullOrEmpty(logText)) {
                    Console.WriteLine(logText);
                }

                command.CommandText = commandTextSQL;
                command.CommandTimeout = commandTimeout;
                command.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(logText)) {
                    Console.WriteLine("OK");
                }
            }
        }

        // turn off checks for optimized (faster) data imports, then turn on again
        public void ExecuteChecks(bool on, string database) {
            int checks = on ? 1 : 0;
            string checksOnOffSql = string.Format("USE {0}; SET unique_checks={1}; SET foreign_key_checks={1}; ", database, checks);

            ExecuteNonQuery(checksOnOffSql);
        }

        public void RunMySqlImport(string filename, string dbName, bool compressed = false) {

            //TODO: Docker support, e.g.
            //cat backup.sql | docker exec -i CONTAINER /usr/bin/mysql -u root --password=root DATABASE

            Console.Error.WriteLine("Importing file: " + filename);

            string mysqldFile = mysqlBinLocation + @"mysql"; // not mysqld

            bool optimize = true;

            //ProcessStartInfo cmdsi = new ProcessStartInfo();
            //Console.WriteLine("Running: " + cmdsi.FileName + " " + cmdsi.Arguments);
            //Process cmd = Process.Start(cmdsi);

            //TODO: show errors and ouput somewhere (cmd.StandardOutput, etc)

            Process process = new Process();
            process.StartInfo.FileName = mysqldFile;

            //process.StartInfo.Arguments = string.Format("-v -u {0} -p{1} {2}", "root", password, dbname);

            if (string.IsNullOrWhiteSpace(password)) {
                process.StartInfo.Arguments = string.Format("--default-character-set=utf8 -v --host={0} -u {1} --port={2}", server, uid, port);
            } else {
                process.StartInfo.Arguments = string.Format("--default-character-set=utf8 -v --host={0} -u {1} --port={2} -p{3}", server, uid, port, password); // (no space between -p and password)
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;

            try {
                //StreamReader reader;
                Stream inStream;
                if (compressed || filename.EndsWith(".gz")) {
                    GZipStream stream = new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), CompressionMode.Decompress);
                    inStream = stream;
                    //reader = new StreamReader(stream, Encoding.Unicode); // causes lots of question marks
                    //reader = new StreamReader(stream, Encoding.UTF8); 
                } else {
                    //reader = new StreamReader(filename, Encoding.Unicode);
                    //reader = new StreamReader(filename, Encoding.UTF8); 
                    inStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

                process.Start();

                StreamWriter input = process.StandardInput;
                BinaryWriter binaryInput = new BinaryWriter(process.StandardInput.BaseStream);

                //char[] buffer = new char[4096];
                byte[] buffer = new byte[4096];
                int count;
                //using (reader)
                using (inStream) {
                    if (!string.IsNullOrWhiteSpace(dbName)) {
                        input.WriteLine("USE " + dbName + "; ");
                    }
                    if (optimize) {
                        //input.WriteLine("SET autocommit=0; ");
                        input.WriteLine("SET unique_checks=0; ");
                        input.WriteLine("SET foreign_key_checks=0; ");
                        input.Flush();
                    }
                    //while ((count = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                    while ((count = inStream.Read(buffer, 0, buffer.Length)) > 0) {
                        if (process.HasExited == true)
                            throw new Exception("DB went away.");

                        //input.Write(buffer, 0, count);
                        //input.Flush();

                        binaryInput.Write(buffer, 0, count);
                        binaryInput.Flush();
                    }
                }

                binaryInput.Flush();

                if (optimize) {
                    //input.WriteLine("COMMIT; ");
                    input.WriteLine("SET unique_checks=1; ");
                    input.WriteLine("SET foreign_key_checks=1; ");
                    input.Flush();
                }

                process.Close();
            } catch (Exception ex) {
                Console.Error.WriteLine("Error importing file: " + filename);
                Console.Error.WriteLine(ex.Message);
            }
        }



        //warning: fails due to memory running out.
        //TODO: replace with something like this: https://stackoverflow.com/questions/13648523/how-to-import-large-sql-file-using-mysql-exe-through-streamreader-standardinp
        public static void ImportSmallDatabaseFile(string filename, bool compressed = true) {
            CatalogueOfLifeDatabase.Instance().CreateWikiDatabase();

            using (MySqlConnection connection = CatalogueOfLifeDatabase.Instance().Connection()) {
                using (MySqlCommand command = new MySqlCommand()) {
                    command.Connection = connection;
                    using (MySqlBackup mb = new MySqlBackup(command)) {
                        StreamReader reader;
                        if (compressed || filename.EndsWith(".gz")) {
                            GZipStream stream = new GZipStream(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), CompressionMode.Decompress);
                            reader = new StreamReader(stream, Encoding.Unicode); //utf8 causes duplicate entry errors
                        } else {
                            reader = new StreamReader(filename, Encoding.Unicode);
                        }
                        command.CommandTimeout = 900;
                        //connection.Open();
                        //mb.ImportFromTextReader(new StringReader("USING );
                        mb.ImportFromTextReader(reader);
                        connection.Close();
                        reader.Close();
                        //command.ExecuteReader();

                        //MySqlBulkLoader loader = new MySqlBulkLoader(connection);
                        //loader.
                    }
                }
            }

        }
    }
}
