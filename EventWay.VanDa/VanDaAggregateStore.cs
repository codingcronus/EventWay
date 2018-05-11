using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventWay.Core;

namespace EventWay.VanDa
{
    public class VanDaAggregateStore : AggregateStore, IExtendedAggregateStore
    {
        public VanDaAggregateStore(
            IAggregateRepository aggregateRepository,
            VanDaEventListener eventListener,
            IAggregateTracking aggregateTracking = null,
            IAggregateCache aggregateCache = null) : base(aggregateRepository, eventListener, aggregateTracking, aggregateCache)
        {
        }

        public async Task Save<T>(IEnumerable<T> aggregates) where T : IAggregate
        {
            var enumeratedAggregates = aggregates.ToArray();

            if (!enumeratedAggregates.Any())
                return;

            _aggregateTracking?.TrackEvents(enumeratedAggregates);

            var orderedEvents = _aggregateRepository.Save(enumeratedAggregates);

            await ((VanDaEventListener)_eventListener).Handle(orderedEvents);

            _aggregateCache?.Set(enumeratedAggregates);
        }
    }
}
