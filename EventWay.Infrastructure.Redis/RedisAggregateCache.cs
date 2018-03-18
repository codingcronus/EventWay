using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EventWay.Core;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace EventWay.Infrastructure.Redis
{
    public class RedisAggregateCache : IAggregateCache
    {
        // TODO: Consider multiple databases...

        private readonly Lazy<ConnectionMultiplexer> _connection;
        private readonly Lazy<IDatabase> _cache;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new RedisContractResolver(),
        };

        public RedisAggregateCache(string connectionString, TextWriter log = null)
        {
            // Postpone connection until needed.
            _connection = new Lazy<ConnectionMultiplexer>(() 
                => ConnectionMultiplexer.Connect(connectionString, log));

            _cache = new Lazy<IDatabase>(() => _connection.Value.GetDatabase());
        }

        public RedisAggregateCache(ConfigurationOptions configuration, TextWriter log = null)
        {
            // Postpone connection until needed.
            _connection = new Lazy<ConnectionMultiplexer>(() 
                => ConnectionMultiplexer.Connect(configuration, log));
        }

        public void Set(IAggregate aggregate)
        {
            Debug.WriteLine($"cache:redis:set:{aggregate.Id}");
            var value = JsonConvert.SerializeObject(aggregate, SerializerSettings);
            _cache.Value.StringSet(aggregate.Id.ToString(), value);
        }

        public void Set<T>(IEnumerable<T> aggregates) where T : IAggregate
        {
            foreach (var aggregate in aggregates)
                Set(aggregate);
        }

        public bool Contains(Guid aggregateId) 
            => _cache.Value.KeyExists(aggregateId.ToString());

        public T Get<T>(Guid aggregateId) where T : IAggregate
        {
            Debug.WriteLine($"cache:redis:get:{aggregateId}");
            var value = _cache.Value.StringGet(aggregateId.ToString());
            var t = JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            return t;
        }

        public bool TryGet<T>(Guid aggregateId, out T aggregate) where T : IAggregate
        {
            var key = aggregateId.ToString();
            var exists = _cache.Value.KeyExists(key);
            aggregate = exists
                ? Get<T>(aggregateId)
                : default(T);
            return exists;
        }

        public void Remove(Guid aggregateId) 
            => _cache.Value.KeyDelete(aggregateId.ToString());

        public void Clear()
        {
            // TODO: CLOUD ENABLE!
           _connection.Value.GetServer("localhost").FlushDatabase();
        }
    }
}
