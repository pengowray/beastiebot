using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beastie.WordNet {
    class SqlunetDatabase {
        public static SqlunetDB Database() {
            //ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["Sqlunet"];
            //return new SqlunetDB(settings.ConnectionString);
            return new SqlunetDB("Sqlunet");

        }
    }
}
