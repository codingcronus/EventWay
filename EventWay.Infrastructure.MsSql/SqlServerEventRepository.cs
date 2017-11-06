using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EventWay.Core;

namespace EventWay.Infrastructure.MsSql
{
    public class SqlServerEventRepository : IEventRepository
    {
        const int CommandTimeout = 600;

        private readonly string _connectionString;

        public SqlServerEventRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public List<OrderedEventPayload> GetEvents<TAggregate>(long from) where TAggregate : Aggregate
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var aggregateType = typeof(TAggregate).Name;
                const string sql = "SELECT * FROM Events WHERE AggregateType = @aggregateType AND Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateType, from }, commandTimeout: CommandTimeout);

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        //public List<OrderedEventPayload> GetEvents(long from)
        //{
        //    using (var conn = new SqlConnection(_connectionString))
        //    {
        //        const string sql = "SELECT * FROM Events WHERE Ordering > @from";

        //        var listOfEventData = conn.Query<Event>(sql, new { from }, commandTimeout: CommandTimeout);

        //        var events = listOfEventData
        //            .Select(x => x.DeserializeOrderedEvent())
        //            .ToList();

        //        return events;
        //    }
        //}

        public List<OrderedEventPayload> GetEventsByAggregateId(Guid aggregateId)
        {
            return GetEventsByAggregateId(0L, aggregateId);
        }

        public List<OrderedEventPayload> GetEventsByAggregateId(long from, Guid aggregateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE AggregateId=@aggregateId AND Version > @from";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId, from }, commandTimeout: CommandTimeout);

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public List<OrderedEventPayload> GetEventsByType(Type eventType)
        {
            return GetEventsByType(0L, eventType);
        }

        public List<OrderedEventPayload> GetEventsByType(long from, Type eventType)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE EventType=@eventType AND Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { eventType.Name, from }, commandTimeout: CommandTimeout);

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

        public List<OrderedEventPayload> GetEventsByTypes(long from, Type[] eventTypes)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE EventType IN(@eventType) AND Ordering > @from";

                var formattedEventTypes = eventTypes
                    .Select(x => x.Name)
                    .Aggregate((i, j) => i + ",'" + j + "'");

                var listOfEventData = conn.Query<Event>(sql, new { formattedEventTypes, from }, commandTimeout: CommandTimeout);

                var events = listOfEventData
                    .Select(x => x.DeserializeOrderedEvent())
                    .ToList();

                return events;
            }
        }

        public int? GetVersionByAggregateId(Guid aggregateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT MAX(Version) FROM Events WHERE AggregateId=@aggregateId";

                var version = (int?)conn.ExecuteScalar(sql, new { aggregateId }, commandTimeout: CommandTimeout);

                return version;
            }
        }

        public OrderedEventPayload[] SaveEvents(Event[] events)
        {
            if (events.Any() == false)
                return new OrderedEventPayload[] { }; // Nothing to save

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    // Save events
                    const string insertSql = @"INSERT INTO Events(EventId, Created, EventType, AggregateType, AggregateId, Version, Payload, Metadata) VALUES (@EventId, @Created, @EventType, @AggregateType, @AggregateId, @Version, @Payload, @Metadata)";
                    conn.Execute(insertSql, events, tx, commandTimeout: CommandTimeout);

                    // Get ordered events
                    //const string selectSql = "SELECT * FROM Events WHERE EventId IN (@eventIds) ORDER BY Ordering";

                    var selectQuery = "SELECT * FROM Events WHERE EventId IN (";
                    for (int i = 0; i < events.Length; i++)
                    {
                        if (i > 0)
                            selectQuery += ", ";
                        selectQuery += "'" + events[i].EventId + "'";
                    }
                    selectQuery += ") ORDER BY Ordering";

                    var listOfEventData = conn.Query<Event>(selectQuery, null, tx, commandTimeout: CommandTimeout);

                    //var listOfEventData = conn.Query<Event>(selectSql, new { eventIds = events.Select(x => x.EventId).ToArray() }, tx);
                    var insertedEvents = listOfEventData
                        .Select(x => x.DeserializeOrderedEvent())
                        .ToList();

                    tx.Commit();

                    return insertedEvents.ToArray();
                }
            }
        }

        public void ClearEvents()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "TRUNCATE TABLE [Events]";
                conn.Execute(sql, commandTimeout: CommandTimeout);
            }
        }
    }
}
