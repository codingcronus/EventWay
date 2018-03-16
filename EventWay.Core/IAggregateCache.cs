using System;

namespace EventWay.Core
{
    public interface IAggregateCache
    {
        void Set<T>(Guid aggregateId, T aggregate) where T : IAggregate;
        bool Contains(Guid aggregateId);
        T Get<T>(Guid aggregateId) where T : IAggregate;
        bool TryGet<T>(Guid aggregateId, out T aggregate) where T : IAggregate;
        void Clear(Guid aggregateId);
    }
}