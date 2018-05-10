using System;
using System.Threading.Tasks;

namespace EventWay.Core
{
    public interface IAggregateStore
    {
        T GetById<T>(Guid aggregateId) where T : IAggregate;
        Task Save(IAggregate aggregate);
    }
}