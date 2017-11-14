using System.Data;
using System.Data.SQLite;

namespace EventWay.Infrastructure.Sqlite
{
    public static class Extensions
    {
        public static SQLiteConnection AsOpen(this SQLiteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }
    }
}
