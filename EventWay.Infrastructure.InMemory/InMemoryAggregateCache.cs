using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EventWay.Core;

namespace EventWay.Infrastructure.InMemory
{
    public class InMemoryAggregateCache : IAggregateCache
    {
        private readonly ConcurrentDictionary<Guid, IAggregate> _cache = new ConcurrentDictionary<Guid, IAggregate>();

        public void Set(IAggregate aggregate) => _cache[aggregate.Id] = aggregate;

        public void Set<T>(IEnumerable<T> aggregates) where T : IAggregate
        {
            foreach (var aggregate in aggregates)
                Set(aggregate);
        }

        public bool Contains(Guid aggregateId) => _cache.ContainsKey(aggregateId);

        public T Get<T>(Guid aggregateId) where T : IAggregate => (T)_cache[aggregateId];

        public bool TryGet<T>(Guid aggregateId, out T aggregate) where T : IAggregate
        {
            aggregate = _cache.ContainsKey(aggregateId)
                ? (T)_cache[aggregateId] 
                : default(T);
            return _cache.ContainsKey(aggregateId);
        }

        public void Remove(Guid aggregateId) => _cache.TryRemove(aggregateId, out var _);

        public void Clear() => _cache.Clear();
    }
}