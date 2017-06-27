using System;
using System.Threading.Tasks;

namespace EventWay.Core
{
    public interface IAggregateRepository
    {
        T GetById<T>(Guid id) where T : IAggregate;
        OrderedEventPayload[] Save(IAggregate aggregate);
    }
}