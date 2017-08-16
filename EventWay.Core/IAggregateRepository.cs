using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventWay.Core
{
    public interface IAggregateRepository
    {
        T GetById<T>(Guid id) where T : IAggregate;
        OrderedEventPayload[] Save(IAggregate aggregate);
        OrderedEventPayload[] Save<T>(IEnumerable<T> aggregates) where T : IAggregate;
    }
}