using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.Wiktionary {
    public class DockerWikiDatabase : MyDatabase {
        public DockerWikiDatabase() {
            port = "3306";
            password = ""; // mysql password. TODO: command line parameter to override blank password
            server = "10.0.75.1";
            database = ""; // default database. have generally left blank and used "USE mydatabase;" in sql.
            uid = "root";
        }
    }
}
