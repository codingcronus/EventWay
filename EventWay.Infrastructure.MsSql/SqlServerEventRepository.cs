using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using EventWay.Core;

namespace EventWay.Infrastructure.MsSql
{
    public class SqlServerEventRepository : IEventRepository
    {
        const int CommandTimeout = 600;
        private const string SchemaName = "dbo";
        private const string TableName = "Events";
        private static string Table => $"{SchemaName}.{TableName}";

        private readonly string _connectionString;

        public SqlServerEventRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public List<OrderedEventPayload> GetEvents<TAggregate>(long from) where TAggregate : Aggregate
        {
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var aggregateType = typeof(TAggregate).Name;
                var sql = $"SELECT * FROM {Table} WHERE AggregateType = @aggregateType AND Ordering > @from";

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
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var sql = $"SELECT * FROM {Table} WHERE AggregateId=@aggregateId AND Version > @from";

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
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var sql = $"SELECT * FROM {Table} WHERE EventType=@eventType AND Ordering > @from";

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
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var sql = $"SELECT * FROM {Table} WHERE EventType IN(@eventType) AND Ordering > @from";

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
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var sql = $"SELECT MAX(Version) FROM {Table} WHERE AggregateId=@aggregateId";

                var version = (int?)conn.ExecuteScalar(sql, new { aggregateId }, commandTimeout: CommandTimeout);

                return version;
            }
        }

        public void ClearEvents()
        {
            using (var conn = new SqlConnection(_connectionString).AsOpen())
            {
                var sql = $"TRUNCATE TABLE {Table}";
                conn.Execute(sql, commandTimeout: CommandTimeout);
            }
        }

        public OrderedEventPayload[] SaveEvents(Event[] events)
        {
            return events.Any()
                ? new BulkCopyTools(_connectionString, TableName).BulkInsertEvents(events)
                : new OrderedEventPayload[] { };
        }
    }
}
