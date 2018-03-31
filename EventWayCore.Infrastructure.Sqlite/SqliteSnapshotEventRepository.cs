using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EventWayCore.Core;
using SQLite;

namespace EventWayCore.Infrastructure.Sqlite
{
    public class SqliteSnapshotEventRepository : ISnapshotEventRepository
    {
        private const int CommandTimeout = 600;
        private readonly string _connectionString;
        private const string TableName = "SnapshotEvents";

        public SqliteSnapshotEventRepository(string connectionString)
        {
            {
                // In-memory:  = @"Data Source=:memory:"
                //var builder = new SQLiteConnectionStringBuilder(connectionString)
                //{
                //    BinaryGUID = false
                //};

                //_connectionString = builder.ConnectionString;

                _connectionString = connectionString;
                // Then you add follow line at where you app beigns to run.
                SqlMapper.AddTypeHandler(new GuidAsCharHandler());
            }
        }

        private SQLiteConnection Connect() => new SQLiteConnection(_connectionString);

        public List<OrderedEventPayload> GetSnapshotEventsByAggregateId(Guid aggregateId)
        {
            using (var conn = Connect())
            {
                var sql = $"SELECT * FROM {TableName} WHERE AggregateId=@aggregateId";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public object GetSnapshotEventByAggregateIdAndVersion(Guid aggregateId, int version)
        {
            using (var conn = Connect())
            {
                var sql = $"SELECT * FROM {TableName} WHERE AggregateId=@aggregateId And Version=@version";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId, version });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events.First().EventPayload;
            }
        }

        public int? GetVersionByAggregateId(Guid aggregateId)
        {
            using (var conn = Connect())
            {
                var sql = $"SELECT MAX(Version) FROM {TableName} WHERE AggregateId=@aggregateId";

                var version = conn.ExecuteScalar<string>(sql, new { aggregateId });

                return version == null
                    ? null
                    : (int?)Convert.ToInt32(version);
            }
        }

        public void SaveSnapshotEvent(Event snapshotEvent)
        {
            SaveSnapshotEvents(new[] { snapshotEvent });
        }

        public void SaveSnapshotEvents(Event[] eventsToSave)
        {
            if (!eventsToSave.Any())
                return;

            using (var conn = Connect())
            {
                var insertSql =
                    $"INSERT INTO {TableName}(EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, Metadata) VALUES (@EventId, @Created, @EventType, @AggregateType, @AggregateId, @Version, @Payload, @Metadata)";
                conn.Execute(insertSql, eventsToSave);
            }
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId, int to)
        {
            using (var conn = Connect())
            {
                var sql = $"DELETE FROM {TableName} WHERE AggregateId=@aggregateId And Version < @to";
                conn.Execute(sql, new { aggregateId, to });
            }
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId)
        {
            using (var conn = Connect())
            {
                var sqlTruncate = $"DELETE FROM {TableName} Where AggregateId=@aggregateId";
                conn.Execute(sqlTruncate, new { aggregateId });
            }
        }

        public void ClearSnapshotEvents()
        {
            using (var conn = Connect())
            {
                var sqlTruncate = $"DELETE FROM {TableName}";
                var sqlResetIndex = $"DELETE FROM sqlite_sequence WHERE name = '{TableName}'";
                conn.Execute(sqlTruncate);
                conn.Execute(sqlResetIndex);
            }
        }
    }
}