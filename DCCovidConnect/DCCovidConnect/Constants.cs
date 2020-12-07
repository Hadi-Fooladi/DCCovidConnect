using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DCCovidConnect
{
    public static class Constants
    {
        public static readonly string DatabaseFilename = "InfoSQLite.db3";

        public static readonly SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }
}
