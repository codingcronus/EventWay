using System.Collections.Generic;
using System.Threading.Tasks;
using EventWay.Core;

namespace EventWay.VanDa
{
    public interface IExtendedAggregateStore : IAggregateStore
    {
        Task Save<T>(IEnumerable<T> aggregates) where T : IAggregate;
    }
}
