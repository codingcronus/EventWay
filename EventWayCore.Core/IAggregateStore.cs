using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventWayCore.Core
{
    public interface IAggregateStore
    {
        T GetById<T>(Guid aggregateId) where T : IAggregate;
        Task Save(IAggregate aggregate);
        Task Save<T>(IEnumerable<T> aggregates) where T : IAggregate;
    }
}