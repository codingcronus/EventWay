using System;
using EventWay.Core;

namespace EventWay.Infrastructure
{
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public T Create<T>(Guid aggregateId, object[] events) where T : IAggregate
        {
            IAggregate aggregate = (T)Activator.CreateInstance(typeof(T));

            aggregate.Id = aggregateId;

            foreach (var @event in events)
                aggregate.Apply(@event);

            return (T)aggregate;
        }
    }
}
