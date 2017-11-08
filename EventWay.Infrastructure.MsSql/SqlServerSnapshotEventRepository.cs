using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EventWay.Core;

namespace EventWay.Infrastructure.MsSql
{
    public class SqlServerSnapshotEventRepository : ISnapshotEventRepository
    {
        private const int CommandTimeout = 600;

        private readonly string _connectionString;

        public SqlServerSnapshotEventRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public List<OrderedEventPayload> GetSnapshotEventsByAggregateId(Guid aggregateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM SnapshotEvents WHERE AggregateId=@aggregateId";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId }, commandTimeout: CommandTimeout);

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public object GetSnapshotEventByAggregateIdAndVersion(Guid aggregateId, int version)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM SnapshotEvents WHERE AggregateId=@aggregateId And Version=@version";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId, version }, commandTimeout: CommandTimeout);

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events.First().EventPayload;
            }
        }

        public int? GetVersionByAggregateId(Guid aggregateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT MAX(Version) FROM SnapshotEvents WHERE AggregateId=@aggregateId";

                var version = (int?)conn.ExecuteScalar(sql, new { aggregateId }, commandTimeout: CommandTimeout);

                return version;
            }
        }

        public void SaveSnapshotEvent(Event snapshotEvent)
        {
            SaveSnapshotEvents(new []{snapshotEvent});
        }

        public void SaveSnapshotEvents(Event[] snapshotEvents)
        {
            if (!snapshotEvents.Any())
                return;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    // Save events
                    const string insertSql = @"INSERT INTO SnapshotEvents(EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, Metadata) VALUES (@EventId, @Created, @EventType, @AggregateType, @AggregateId, @Version, @Payload, @Metadata)";
                    conn.Execute(insertSql, snapshotEvents, tx, commandTimeout: CommandTimeout);
                    tx.Commit();
                }
            }
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId, int to)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "DELETE FROM SnapshotEvents WHERE AggregateId=@aggregateId And Version < @from";
                conn.Execute(sql, new {aggregateId, to}, commandTimeout: CommandTimeout);
            }
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId)
        {
            ClearSnapshotEventsByAggregateId(aggregateId, int.MaxValue);
        }

        public void ClearSnapshotEvents()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "TRUNCATE TABLE SnapshotEvents";
                conn.Execute(sql, commandTimeout: CommandTimeout);
            }
        }
    }
}