using System;

namespace EventWayCore.Core
{
    public interface IAggregateFactory
    {
        T Create<T>(Guid aggregateId, object[] events) where T : IAggregate;
        int GetSnapshotSize<T>() where T : IAggregate;
    }
}