using PingTool.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace PingTool.Helpers
{
    public static class SQLiteHelper
    {
        private static string dbPath = string.Empty;
        private static string DbPath
        {
            get
            {
                if (string.IsNullOrEmpty(dbPath))
                {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pingdb.sqlite");
                }
                return dbPath;
            }
        }
        private static SQLiteConnection DbConnection
        {
            get
            {
                return new SQLiteConnection(new SQLiteConnectionString(DbPath));
            }
        }
        public static void InitializeDatabase()
        {
            using (var db = DbConnection)
            {
                var result = db.CreateTable<PingMassage>();
                if (result == CreateTableResult.Created)
                {
                }
            }
        }
        public static void Delete(PingMassage post)
        {
            using (var db = DbConnection)
            {
                db.Delete(post);
            }
        }
        public static IEnumerable<PingMassage> GetAll(Guid? pingId = null)
        {
            using (var db = DbConnection)
            {
                var quary = db.Table<PingMassage>();
                if (pingId != null)
                {
                    quary = quary.Where(x => x.PingId == pingId);
                }
                return quary.ToList();
            }
        }
        public static PingMassage Get(int Id)
        {
            using (var db = DbConnection)
            {
                return db.Table<PingMassage>().FirstOrDefault(x => x.Id == Id);
            }
        }
        public static void Save(PingMassage post)
        {
            using (var db = DbConnection)
            {
                db.Insert(post);
            }
        }
        public static IEnumerable<PingMassage> GetAllDistinct()
        {
            using (var db = DbConnection)
            {
                return db.Table<PingMassage>().GroupBy(x => x.PingId).Select(x => x.First()).ToList();
            }
        }
    }
}
