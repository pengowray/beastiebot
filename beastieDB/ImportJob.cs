using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;

// import/job tools

namespace beastie.beastieDB {
    // also: DeleteOld, DeleteAll, ContinueOnly, DeleteOldAndStart, StartThenReplace ...
    public enum JobAction { None, ContinueOrStartNewIfNone, StartNewIfNone, ForceStartNew }
    
    public static class ImportJob {

        public static DataImport StartImport(this BeastieDB db, JobAction jobAction, string fnName, string source, string destinationTable = null, long? parentImportId = null) {

            if (jobAction == JobAction.None)
                throw new ArgumentException();

            DataImport existing = null;

            if (jobAction == JobAction.ContinueOrStartNewIfNone || jobAction == JobAction.StartNewIfNone) {
                existing = db.DataImports.Where(r =>
                        r.fn == fnName
                        && r.source == source
                        && r.table == destinationTable
                        && r.date_del == null // FIXME
                        )
                        .OrderByDescending(r => r.date_start)
                        .FirstOrDefault();
            }

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

        public static void ImportSuccess(this DataImport dataImport, BeastieDB db) {
            dataImport.date_complete = DateTime.UtcNow.Ticks;
            dataImport.Log(db, "Success", false);
            db.Update(dataImport);

        }

        public static void CleanOutdatedImports(this BeastieDB db) {

        }

        public static void CleanIncompleteImports(this BeastieDB db) {

        }

        public static DataImport StartChildImport(this DataImport dataImport, JobAction jobAction, string fnName, string source, string destinationTable = null) {
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

        //////////////////// print reports

        public static void ListJobs(long? parentid = null) {
            var db = BeastieDatabase.Database();

            Console.WriteLine("Incomplete imports:");
            var incompletes = db.DataImports.Where(r => r.parent_dataimport == parentid && r.date_del == null && r.date_complete == null).OrderBy(r => r.fn).ThenByDescending(r => r.source).ThenByDescending(r => r.id);
            string lastSourcenFn = null;
            foreach (var dataImport in incompletes) {
                bool isNewSource = (lastSourcenFn != dataImport.SourcenFn());
                dataImport.Print(db, isNewSource);
                lastSourcenFn = dataImport.SourcenFn();
            }
            Console.WriteLine();


            var completes = db.DataImports.Where(r => r.parent_dataimport == parentid && r.date_del == null && r.date_complete != null).OrderBy(r => r.fn).ThenByDescending(r => r.source).ThenByDescending(r => r.id);
            Console.WriteLine("Completed imports:");
            foreach (var dataImport in completes) {
                bool isNewSource = (lastSourcenFn != dataImport.SourcenFn());
                dataImport.Print(db, isNewSource);
                lastSourcenFn = dataImport.SourcenFn();
            }
            Console.WriteLine();

            var deleted = db.DataImports.Where(r => r.parent_dataimport == parentid && r.date_del != null).OrderBy(r => r.fn).ThenByDescending(r => r.source).ThenByDescending(r => r.id);
            Console.WriteLine("Deletable imports:");
            foreach (var dataImport in deleted) {
                bool isNewSource = (lastSourcenFn != dataImport.SourcenFn());
                dataImport.Print(db, isNewSource);
                lastSourcenFn = dataImport.SourcenFn();
            }


            Console.WriteLine();
            Console.WriteLine("* = most recent of type");
        }

        public static void DeleteJob(long jobid, bool markOnly = false, bool keepMainEntry = false) {
            var db = BeastieDatabase.Database();
            var dataImport = db.DataImports.Find(jobid);

            DoMarkJob(dataImport, db);

            if (markOnly) {
                Console.WriteLine("Done. Deletion date added to old jobs.");
                return;
            }

            DoDeleteJob(dataImport, db, keepMainEntry);

        }

        public static void CleanJobs(BeastieDB db = null, bool markOnly = false, bool keepMainEntry = false) {
            if (db == null)
                db = BeastieDatabase.Database();

            Console.WriteLine("Cleaning out old jobs");

            //db.BeginTransaction(); 
            DoMarkDeletableJobs(db); // starts new db connections, so transaction is probably meaningless
            //db.CommitTransaction();

            //TODO: delete from table
            if (markOnly) {
                Console.WriteLine("Done. Deletion date added to old jobs.");
                return;
            }

            Console.WriteLine("Deleting old jobs");
            //db.BeginTransaction();
            DoDeleteOldJobs(db, keepMainEntry);
            //db.CommitTransaction();

        }


        /// <summary>
        /// Add a deletion date to old jobs which can be deleted
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parentID"></param>
        /// <param name="depth"></param>
        static void DoMarkDeletableJobs(BeastieDB db=null, long? parentID = null, int depth = 0) {
            if (db == null)
                db = BeastieDatabase.Database();

            var db2 = BeastieDatabase.Database();

            int MAX_DEPTH = 8;
            if (depth > MAX_DEPTH) {
                Console.Error.WriteLine("Too deep! Possible infinite loop? Parent id=" + parentID, " depth=" + depth);
                throw new ArgumentException("Exceeded maximum recursion depth.");
            }

            var incompletes = db.DataImports.Where(r => r.parent_dataimport == parentID && r.date_del == null && r.date_complete == null).OrderBy(r => r.fn).ThenByDescending(r => r.source).ThenByDescending(r => r.id).ToArray();
            string lastSourcenFn = null;
            foreach (var dataImport in incompletes) {
                bool isNewSource = (lastSourcenFn != dataImport.SourcenFn());
                //dataImport.Print(db, isNewSource);
                if (!isNewSource) {
                    DoMarkJob(dataImport, db, depth);
                }
                lastSourcenFn = dataImport.SourcenFn();
            }
        }

        static void DoMarkJob(DataImport dataImport, BeastieDB db = null, int depth = 0) {
            if (db == null)
                db = BeastieDatabase.Database();

            dataImport.date_del = DateTime.UtcNow.Ticks;
            db.Update(dataImport);

            DoMarkDeletableJobs(db, dataImport.id, depth + 1); //child jobs (recursive)
        }


        /// <summary>
        /// Delete jobs which have a deletion date. 
        /// If "keepEntry" then only delete the records in DataImports.table but keep the DataImport row (job) itself. 
        /// Child import entries will not be kept. (todo: make optional?)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parentID"></param>
        /// <param name="depth"></param>
        static void DoDeleteOldJobs(BeastieDB db = null, bool keepEntry = false, long? parentID = null, int depth = 0) {
            if (db == null)
                db = BeastieDatabase.Database();

            int MAX_DEPTH = 8;
            if (depth > MAX_DEPTH) {
                Console.Error.WriteLine("Too deep!! Possible infinite loop? Parent id=" + parentID, " depth=" + depth);
                throw new ArgumentException("Exceeded maximum recursion depth.");
            }

            var deletable = db.DataImports.Where(r => r.parent_dataimport == parentID && r.date_del != null).OrderBy(r => r.id).ToArray();
            foreach (var dataImport in deletable) {
                //dataImport.Print(db, isNewSource);
                //TODO: recycle db connections?
                DoDeleteJob(dataImport, db, keepEntry, depth);
            }

        }

        public static void DoDeleteJob(DataImport dataImport, BeastieDB db = null, bool keepEntry = false, int depth = 0) {

            DoDeleteOldJobs(null, false, dataImport.id, depth + 1); //child jobs (recursive) -- do first

            // delete imported data
            if (!string.IsNullOrWhiteSpace(dataImport.table) && dataImport.table.ToLower() != "dataimport") {
                var command = db.CreateCommand();
                command.CommandText = string.Format("DELETE FROM `{0}` WHERE `dataimport`={1};", dataImport.table, dataImport.id); //TODO: don't use sql?
                command.ExecuteNonQuery();
            }

            // delete import record
            if (!keepEntry) {
                db.Delete(dataImport);
            }

        }

        public static string SourcenFn(this DataImport di) {
            // source and function combined
            return di.fn + "(" + di.source + ")";
        }

        public static void Print(this DataImport di, BeastieDB db, bool star = false) {
            string starText = (star ? " * " : "   ");
            //ignored for now: 
            // di.parameters,  // unused additional params
            // bool isContinuing = (di.continuing == 1); // internal use (set to true first time a job continues)

            //TODO: warning: unsafe/unescaped SQL. Do not call externally.
            string rowsImported = "-";
            if (di.table != null) {
                var command = db.CreateCommand();
                command.CommandText = string.Format("SELECT COUNT(id) AS count FROM `{0}` WHERE `dataimport`={1};", di.table, di.id);
                rowsImported = command.ExecuteScalar().ToString();
                command.Dispose();
            }

            var db2 = BeastieDatabase.Database();
            var childJobs = db2.DataImports.Where(r => r.parent_dataimport == di.id).Count();
            //string childJobs = "";

            Console.WriteLine("{0}{1}\t{2}\t{3}\t{4}\trows={5}", starText, di.id, di.SourcenFn(), di.table, DateToText(di.date_start), rowsImported);

            if (childJobs > 0) {
                Console.WriteLine("   └ " + childJobs + " child job(s)");
            }

        }

        public static string DateToText(long? ticks) {
            if (ticks == null)
                return "-";

            return new DateTime((long)ticks, DateTimeKind.Utc).ToString("s");
        }
    }
}
