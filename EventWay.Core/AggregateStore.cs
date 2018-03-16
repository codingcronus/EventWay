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
            return _aggregateCache.TryGet<T>(aggregateId, out var aggregate) 
                ? aggregate : 
                _aggregateRepository.GetById<T>(aggregateId);
        }

		public async Task Save(IAggregate aggregate)
		{
			if (_aggregateTracking != null)
			{
				_aggregateTracking.TrackEvents(aggregate);
			}

			var orderedEvents = _aggregateRepository.Save(aggregate);

			foreach (var @event in orderedEvents)
			{
				await _eventListener.Handle(@event);
			}
		}

		public async Task Save<T>(IEnumerable<T> aggregates) where T : IAggregate
		{
			if (!aggregates.Any())
			{
				return;
			}

			if (_aggregateTracking != null)
			{
				_aggregateTracking.TrackEvents(aggregates);
			}
			var orderedEvents = _aggregateRepository.Save(aggregates);

			await _eventListener.Handle(orderedEvents);
		}
	}
}