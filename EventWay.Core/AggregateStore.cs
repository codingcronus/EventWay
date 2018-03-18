using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventWay.Core
{
	public class AggregateStore : IAggregateStore
	{
		private readonly IAggregateRepository _aggregateRepository;
		private readonly IAggregateTracking _aggregateTracking;
		private readonly IEventListener _eventListener;
	    private readonly IAggregateCache _aggregateCache;

        public AggregateStore(
		    IAggregateRepository aggregateRepository, 
		    IEventListener eventListener, 
		    IAggregateTracking aggregateTracking = null,
		    IAggregateCache aggregateCache = null)
		{
			if (aggregateRepository == null) throw new ArgumentNullException(nameof(aggregateRepository));
			if (eventListener == null) throw new ArgumentNullException(nameof(eventListener));

			_aggregateRepository = aggregateRepository;
			_aggregateTracking = aggregateTracking;
			_eventListener = eventListener;
		    _aggregateCache = aggregateCache;
        }

        public T GetById<T>(Guid aggregateId) where T : IAggregate
        {
            if (_aggregateCache != null)
            {
                if (_aggregateCache.TryGet<T>(aggregateId, out var aggregate))
                    return aggregate;
            }

            return _aggregateRepository.GetById<T>(aggregateId);
        }

		public async Task Save(IAggregate aggregate)
		{
		    var a = new IAggregate[] {aggregate};
		    await Save(a);
		}

		public async Task Save<T>(IEnumerable<T> aggregates) where T : IAggregate
		{
		    var enumeratedAggregates = aggregates.ToArray();

		    if (!enumeratedAggregates.Any())
		        return;

		    _aggregateTracking?.TrackEvents(enumeratedAggregates);

		    var orderedEvents = _aggregateRepository.Save(enumeratedAggregates);

			await _eventListener.Handle(orderedEvents);
        
		    _aggregateCache?.Set(enumeratedAggregates);
		}
	}
}