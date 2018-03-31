using System;
using System.Reflection;
using EventWayCore.Core;

namespace EventWayCore.Infrastructure
{
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public int GetSnapshotSize<T>() where T : IAggregate
        {
            var aggregate = Create<T>(Guid.NewGuid(), null);

            return aggregate.SnapshotSize;
        }

        public T Create<T>(Guid aggregateId, object[] events) where T : IAggregate
        {
            IAggregate aggregate = (T)Activator.CreateInstance(typeof(T),
                BindingFlags.CreateInstance |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.OptionalParamBinding,
                null,
                new object[] { aggregateId },
                null);

            aggregate.Id = aggregateId;
            
            if (events != null)
                foreach (var @event in events)
                    aggregate.Apply(@event);

            return (T)aggregate;
        }
    }
}
