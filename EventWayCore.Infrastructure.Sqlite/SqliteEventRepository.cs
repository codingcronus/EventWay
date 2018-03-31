using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using EventWayCore.Core;
using SQLite;

namespace EventWayCore.Infrastructure.Sqlite
{
    public class SqliteEventRepository : IEventRepository
    {
        private const int CommandTimeout = 600;
        private readonly string _connectionString;
        private const string TableName = "Events";

        public SqliteEventRepository(string connectionString)
        {
            // In-memory:  = @"Data Source=:memory:"
            //var builder = new SQLiteConnectionStringBuilder(connectionString)
            //{
            //    BinaryGUID = false
            //};

            _connectionString = connectionString; //builder.ConnectionString;

            // Then you add follow line at where you app beigns to run.
            SqlMapper.AddTypeHandler(new GuidAsCharHandler());
        }

        public List<OrderedEventPayload> GetEvents<TAggregate>(long @from) where TAggregate : Aggregate
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var aggregateType = typeof(TAggregate).Name;
                var sql = $"SELECT * FROM {TableName} WHERE AggregateType = @aggregateType AND Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateType, from });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public List<OrderedEventPayload> GetEventsByAggregateId(Guid aggregateId)
        {
            return GetEventsByAggregateId(0L, aggregateId);
        }

        public List<OrderedEventPayload> GetEventsByAggregateId(long @from, Guid aggregateId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sql = $"SELECT * FROM {TableName} WHERE AggregateId=@aggregateId AND Version > @from";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId, from });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public List<OrderedEventPayload> GetEventsByType(Type eventType)
        {
            return GetEventsByType(0, eventType);
        }

        public List<OrderedEventPayload> GetEventsByType(long @from, Type eventType)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sql = $"SELECT * FROM {TableName} WHERE EventType=@eventType AND Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { eventType.Name, from });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public List<OrderedEventPayload> GetEventsByTypes(Type[] eventTypes)
        {
            return GetEventsByTypes(0L, eventTypes);
        }

        public List<OrderedEventPayload> GetEventsByTypes(long @from, Type[] eventTypes)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sql = $"SELECT * FROM {TableName} WHERE EventType IN(@eventType) AND Ordering > @from";

                var formattedEventTypes = eventTypes
                    .Select(x => x.Name)
                    .Aggregate((i, j) => i + ",'" + j + "'");

                var listOfEventData = conn.Query<Event>(sql, new { formattedEventTypes, from });

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public int? GetVersionByAggregateId(Guid aggregateId)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sql = $"SELECT MAX(Version) FROM {TableName} WHERE AggregateId=@aggregateId";

                var version = conn.ExecuteScalar<string>(sql, aggregateId);

                return version == null
                    ? null
                    : (int?)Convert.ToInt32(version);
            }
        }

        public OrderedEventPayload[] SaveEvents(Event[] eventsToSave)
        {
            if (eventsToSave.Any() == false)
                return new OrderedEventPayload[] { }; // Nothing to save

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.BeginTransaction();
                const string insertSql =
                    @"INSERT INTO Events(EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, Metadata) VALUES (@EventId, @Created, @EventType, @AggregateType, @AggregateId, @Version, @Payload, @Metadata)";
                conn.Execute(insertSql, eventsToSave);

                var selectQuery = $"SELECT * FROM Events ORDER BY Ordering DESC LIMIT {eventsToSave.Length}";
                var listOfEventData = conn.Query<Event>(selectQuery, null);

                var insertedEvents = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                conn.Commit();
                return insertedEvents.ToArray();
            }
        }

        public void ClearEvents()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sqlTruncate = $"DELETE FROM {TableName}";
                var sqlResetIndex = $"DELETE FROM sqlite_sequence WHERE name = '{TableName}'";
                conn.Execute(sqlTruncate);
                conn.Execute(sqlResetIndex);
            }
        }
    }
}
