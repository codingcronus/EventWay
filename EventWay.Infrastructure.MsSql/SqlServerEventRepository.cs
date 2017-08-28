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
        private readonly string _connectionString;

        public SqlServerEventRepository(string connectionString, bool createEventsTable = false)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;

            if (createEventsTable)
            {
                //TODO: Create Events Table
                /*
                 CREATE TABLE Events(
                        Ordering bigint not null IDENTITY(1,1) PRIMARY KEY,
		                EventId uniqueidentifier not null,
		                Created datetime not null,
                        EventType nvarchar(100) not null,
                        AggregateType nvarchar(100) not null,
                        AggregateId uniqueidentifier not null,
                        Version int not null,
                        Payload nvarchar(MAX) not null,
                        MetaData nvarchar(MAX) null,
                        Dispatched bit not null default(0),
                        Constraint AK_EventId UNIQUE(EventId)
                    )


                CREATE INDEX Idx_Events_EventType
                ON Events(EventType)
                GO

                CREATE INDEX Idx_Events_AggregateId
                ON Events(AggregateId)
                GO

                CREATE INDEX Idx_Events_Dispatched
                ON Events(Dispatched)
                GO
                 */
            }
        }

        public List<OrderedEventPayload> GetEvents()
        {
            return GetEvents(0L);
        }

        public List<OrderedEventPayload> GetEvents(long from)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { from });

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

        public List<OrderedEventPayload> GetEventsByAggregateId(long from, Guid aggregateId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE AggregateId=@aggregateId AND Ordering > @from";

                var listOfEventData = conn.Query<Event>(sql, new { aggregateId, from });

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

        public List<OrderedEventPayload> GetEventsByTypes(long from, Type[] eventTypes)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT * FROM Events WHERE EventType IN(@eventType) AND Ordering > @from";

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
            using (var conn = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT MAX(Version) FROM Events WHERE AggregateId=@aggregateId";

                var version = (int?)conn.ExecuteScalar(sql, new { aggregateId });

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
                    conn.Execute(insertSql, events, tx);

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

                    var listOfEventData = conn.Query<Event>(selectQuery, null, tx);

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
                conn.Execute(sql);
            }
        }
    }
}
