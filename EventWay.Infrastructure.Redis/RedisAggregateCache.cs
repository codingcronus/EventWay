using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventWay.Core;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace EventWay.Infrastructure.Redis
{
    public class RedisAggregateCache : IAggregateCache
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        private IDatabase _cache = null;
        public IDatabase Cache
        {
            get
            {
                // TODO: Consider multiple databases...
                return _cache ?? (_cache = _lazyConnection.Value.GetDatabase());
            }
        }

        public RedisAggregateCache(ConfigurationOptions configuration)
        {
            // Postpone connection until needed.
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration));
        }

        public void Set<T>(Guid aggregateId, T aggregate) where T : IAggregate
        {
            Cache.StringSet(aggregateId.ToString(), JsonConvert.SerializeObject(aggregate));
        }

        public bool Contains(Guid aggregateId)
        {
            return Cache.KeyExists(aggregateId.ToString());
        }

        public T Get<T>(Guid aggregateId) where T : IAggregate
        {
            var json = Cache.StringGet(aggregateId.ToString());
            return JsonConvert.DeserializeObject<T>(json);
        }

        public bool TryGet<T>(Guid aggregateId, out T aggregate) where T : IAggregate
        {
            var key = aggregateId.ToString();
            var exists = Cache.KeyExists(key);
            aggregate = exists
                ? Get<T>(aggregateId)
                : default(T);
            return exists;
        }

        public void Remove(Guid aggregateId)
        {
            Cache.KeyDelete(aggregateId.ToString());
        }

        public void Clear()
        {
            // TODO: CLOUD ENABLE!
           _lazyConnection.Value.GetServer("localhost").FlushDatabase();
        }
    }
}
