using System.Data;
using System.Data.SQLite;
using Dapper;

namespace EventWay.Infrastructure.Sqlite
{
    public static class SqliteDbTool
    {
        public static void CreateTables(string connectionString)
        {
            using (var conn = new SQLiteConnection(connectionString).AsOpen())
                conn.Execute(CreateSql);
        }

        public static void CreateTables(SQLiteConnection connection)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            connection.Execute(CreateSql);
        }

        private const string CreateSql = @"CREATE TABLE Events (
    Ordering      INTEGER       PRIMARY KEY AUTOINCREMENT,
    EventId       CHAR (36)     UNIQUE
                                NOT NULL,
    Created       DATETIME      NOT NULL,
    EventType     VARCHAR (450) NOT NULL,
    AggregateType VARCHAR (100) NOT NULL,
    AggregateId   CHAR (36)     NOT NULL,
    Version       INTEGER       NOT NULL,
    Payload       TEXT          NOT NULL,
    Metadata      TEXT,
    Dispatched    BOOLEAN       NOT NULL
                                DEFAULT (0) 
);

CREATE TABLE SnapshotEvents (
    Ordering      BIGINT        PRIMARY KEY,
    EventId       CHAR (36)     UNIQUE
                                NOT NULL,
    Created       DATETIME      NOT NULL,
    EventType     VARCHAR (450) NOT NULL,
    AggregateType VARCHAR (100) NOT NULL,
    AggregateId   CHAR (36)     NOT NULL,
    Version       INTEGER       NOT NULL,
    Payload       TEXT          NOT NULL,
    Metadata      TEXT,
    Dispatched    BOOLEAN       NOT NULL
                                DEFAULT (0) 
);";
    }
}
