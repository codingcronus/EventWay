using System;

namespace EventWay.Core
{
    public interface IAggregateFactory
    {
        T Create<T>(Guid aggregateId, object[] events) where T : IAggregate;
    }
}