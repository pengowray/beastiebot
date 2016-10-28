using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Extensions;
using LinqToDB.Linq;
using LinqToDB.Data;
using LinqToDB.Expressions;
using LinqToDB.DataProvider;
using LinqToDB.Mapping;
using System.Configuration;
using beastie.WordVector;

namespace beastie.beastieDB {

    // https://github.com/linq2db/linq2db

    // example: https://github.com/linq2db/examples/blob/master/SQLite/GetStarted/App.config

    // http://system.data.sqlite.org/ -- ADO.NET provider for SQLite?

    // db file location in App.config (connectionStrings)
    public class BeastieDatabase {

        public static BeastieDB Database() {
            //ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["Beastie"];
            //return new BeastieDB(settings.ConnectionString);
            return new BeastieDB("Beastie");
        }

        public static string NormalizeForWordsData(string rawWord) {
            return rawWord.Replace(" ", "").Replace("-", "").Replace(".", "").ToLowerInvariant().Normalize();
        }
    }


    public static class BeastieExtensions {

        public static void Log(this DataImport dataImport, BeastieDB db, string message, bool andUpdate = true) {
            string fullMessage = string.Format("{0}: {1}\n", DateTime.UtcNow.ToString("s"), message);
            if (dataImport.log == null) {
                dataImport.log = fullMessage;
            } else {
                dataImport.log = dataImport.log + fullMessage;
            }

            if (andUpdate)
                db.Update(dataImport);

        }

        public static SimilarWords DeserializeData(this WordDistancesData distData) {
            if (distData == null)
                return null;

            return Newtonsoft.Json.JsonConvert.DeserializeObject<SimilarWords>(distData.data);
        }

    }

}