using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public interface IAggregateCache
    {
        void Set(IAggregate aggregate);
        void Set<T>(IEnumerable<T> aggregates) where T : IAggregate;
        bool Contains(Guid aggregateId);
        T Get<T>(Guid aggregateId) where T : IAggregate;
        bool TryGet<T>(Guid aggregateId, out T aggregate) where T : IAggregate;
        void Remove(Guid aggregateId);
        void Clear();
    }
}