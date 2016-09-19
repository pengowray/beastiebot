using beastie.DataModel;
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

namespace beastie.beastieDB {

    // https://github.com/linq2db/linq2db

    // example: https://github.com/linq2db/examples/blob/master/SQLite/GetStarted/App.config

    // http://system.data.sqlite.org/ -- ADO.NET provider for SQLite?

    // db file location in App.config (connectionStrings)
    public class BeastieDatabase {

        public static BeastieDB Database() {
            return new BeastieDB();
        }

    }

    public static class BeastieExtensions {
        public static DataImport StartImport(this BeastieDB db, JobAction jobAction, string fnName, string source, string destinationTable = null, long? parentImportId = null) {
            if (jobAction == JobAction.ContinueOrStartNewIfNone || jobAction == JobAction.StartNewIfNone) {
                var existing = db.DataImports.Where(r =>
                        r.fn == fnName
                        && r.source == source
                        && r.table == destinationTable
                        && r.date_del == null // FIXME
                        )
                        .OrderByDescending(r => r.date_start)
                        .FirstOrDefault();

                if (existing != null) {
                    if (existing.date_complete != null) {
                        return null; // already complete
                    } else if (jobAction == JobAction.ContinueOrStartNewIfNone) {
                        existing.continuing = 1;
                        //existing.log = existing.log + DateTime.Now + " Continuing..." + "\n";
                        existing.Log(db, "Continuing...", false);
                        db.Update(existing);
                        return existing;
                    }
                }
            }

            var newImport = new DataImport();
            newImport.date_start = DateTime.UtcNow.Ticks;
            newImport.fn = fnName;
            newImport.source = source;
            newImport.table = destinationTable;
            newImport.parent_dataimport = parentImportId;
            newImport.continuing = 0;
            //newImport.log = DateTime.Now + " Starting..." + "\n";
            newImport.Log(db, "Starting", false);

            //db.Insert(newImport);
            newImport.id = Convert.ToInt64(db.InsertWithIdentity(newImport));

            return newImport;
        }

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

        public static DataImport StartSubImport(this DataImport dataImport, JobAction jobAction, string fnName, string source, string destinationTable = null) {
            return BeastieDatabase.Database().StartImport(jobAction, fnName, source, destinationTable, dataImport.id);
        }

        public static DataImport GetLastSuccessfulImport(this BeastieDB db, string importFunction, string source) {
            return db.DataImports
                .Where(r => r.source == source && r.date_del == null && r.date_complete != null)
                .OrderByDescending(r => r.date_complete)
                .FirstOrDefault();
        }


        public static DataImport GetLastIncompleteImport(this BeastieDB db, string source) {
            return db.DataImports
                .Where(r => r.source == source && r.date_del == null && r.date_complete == null)
                .OrderByDescending(r => r.date_start)
                .FirstOrDefault();
        }

        public static void CleanOutdatedImports(this BeastieDB db) {

        }

        public static void CleanIncompleteImports(this BeastieDB db) {

        }


        public static void ImportSuccess(this DataImport dataImport) {
            dataImport.date_complete = DateTime.UtcNow.Ticks;
        }
    }

}