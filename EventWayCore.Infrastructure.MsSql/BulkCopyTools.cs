using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EventWayCore.Core;

namespace EventWayCore.Infrastructure.MsSql
{
    public class BulkCopyTools
    {
        public string TableName { get; }
        public string TableSchema { get; }
        public int CommandTimeout { get; }
        private readonly string _connectionString;
        private readonly string _tableNameWithSchema;

        public BulkCopyTools(
            string connectionString,
            string tableName,
            string tableSchema = "dbo",
            int commandTimeout = 60)
        {
            TableName = tableName;
            TableSchema = tableSchema;
            CommandTimeout = commandTimeout;
            _connectionString = connectionString;
            _tableNameWithSchema = $"{TableSchema}.{TableName}";
        }

        public OrderedEventPayload[] BulkInsertEvents(Event[] events)
        {
            if (events.Any() == false)
                return new OrderedEventPayload[] { }; // Nothing to save

            var tempTableName = $"#{TableName}";

            using (var conn = new SqlConnection(_connectionString).AsOpen())
            using (var transaction = conn.BeginTransaction())
            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
            {
                // Create a temp table to bulk insert into.
                CreateTempTable(conn, transaction, tempTableName);

                // Prepare bulk data from events.
                var rows = EventsToDataRows(events).ToArray();

                // Prepare the bulk copy.
                bulk.DestinationTableName = tempTableName;
                bulk.BatchSize = 50;
                bulk.BulkCopyTimeout = CommandTimeout;

                // Perform bulk copy into temp table.
                bulk.WriteToServer(rows);

                // Move bulk inserted rows into the event table.
                MoveEventsFromTempTableToEventsTable(conn, transaction, tempTableName, _tableNameWithSchema);

                // As the table has an integer identity column, we can now select the 
                // top N rows ordered by "indentity" (Ordering) descending to get the 
                // just inserted events. Where N is the number of events written.
                var sql = $"SELECT TOP {events.Length} * FROM {_tableNameWithSchema} with(NOLOCK) ORDER BY Ordering DESC";
                var listOfEventData = conn.Query<Event>(sql, transaction: transaction,
                    commandTimeout: CommandTimeout);

                transaction.Commit();

                return listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToArray();
            }
        }

        private static void CreateTempTable(
            IDbConnection conn,
            IDbTransaction transaction,
            string tempTableName)
        {
            var sql = string.Join(Environment.NewLine,
                $"IF OBJECT_ID('tempdb..{tempTableName}') IS NOT NULL",
                $"DROP TABLE {tempTableName};",
                $"CREATE TABLE {tempTableName}",
                @"(
    [Ordering] [bigint] NOT NULL,
	[EventId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EventType] [nvarchar](450) NOT NULL,
	[AggregateType] [nvarchar](100) NOT NULL,
	[AggregateId] [uniqueidentifier] NOT NULL,
	[Version] [int] NOT NULL,
	[Payload] [nvarchar](max) NOT NULL,
	[MetaData] [nvarchar](max) NULL)");

            conn.Execute(sql, transaction: transaction);
        }

        private static void MoveEventsFromTempTableToEventsTable(
            IDbConnection conn,
            IDbTransaction transaction,
            string tempTableName,
            string tableName)
        {
            var sql = string.Join(Environment.NewLine,
                $"Insert Into {tableName}",
                "(EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, MetaData)",
                "Select EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, MetaData",
                $"From {tempTableName}",
                "Order By Version");

            conn.Execute(sql, transaction: transaction);
        }

        private static IEnumerable<DataRow> EventsToDataRows<T>(IEnumerable<T> events)
        {
            //we need the type to figure out the properties
            var type = typeof(T);

            //Get the properties of our type
            var properties = type.GetProperties();
            var dataTable = new DataTable("Events");

            // Add data columns.
            foreach (var property in properties)
                dataTable.Columns.Add(property.Name, property.PropertyType);

            foreach (var @event in events)
            {
                var eventValues = properties
                    .Select(x => x.GetValue(@event))
                    .ToArray();
                yield return dataTable.LoadDataRow(eventValues, true);
            }
        }
    }
}
