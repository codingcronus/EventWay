using System;
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
		    _aggregateRepository = aggregateRepository ?? throw new ArgumentNullException(nameof(aggregateRepository));
			_eventListener = eventListener ?? throw new ArgumentNullException(nameof(eventListener));

		    _aggregateTracking = aggregateTracking;
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
		    _aggregateTracking?.TrackEvents(aggregate);

		    var orderedEvents = _aggregateRepository.Save(aggregate);

		    foreach (var @event in orderedEvents)
			    await _eventListener.Handle(@event);
        
		    _aggregateCache?.Set(aggregate);
		}
	}
}