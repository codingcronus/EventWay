using System;
using System.Collections.Generic;

namespace EventWayCore.Core
{
    public interface IAggregateRepository
    {
        T GetById<T>(Guid id) where T : IAggregate;
        OrderedEventPayload[] Save(IAggregate aggregate);
        OrderedEventPayload[] Save<T>(IEnumerable<T> aggregates) where T : IAggregate;
    }
}