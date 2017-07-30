using System;
using System.Collections.Generic;
using System.Linq;

namespace EventWay.Core
{
    public class AggregateRepository : IAggregateRepository
    {
        private readonly IEventRepository _eventRepository;
        private readonly IAggregateFactory _aggregateFactory;

        public AggregateRepository(IEventRepository eventRepository, IAggregateFactory aggregateFactory)
        {
            if (eventRepository == null) throw new ArgumentNullException(nameof(eventRepository));
            if (aggregateFactory == null) throw new ArgumentNullException(nameof(aggregateFactory));

            _eventRepository = eventRepository;
            _aggregateFactory = aggregateFactory;
        }

        public T GetById<T>(Guid aggregateId) where T : IAggregate
        {
            var loadFromEvent = 0L;

            // Check for Snapshots
            var lastEventIndex = _eventRepository.GetVersionByAggregateId(aggregateId);
            var snapshotSize = _aggregateFactory.GetSnapshotSize<T>();
            if (lastEventIndex.HasValue && lastEventIndex.Value >= snapshotSize)
                loadFromEvent = lastEventIndex.Value - lastEventIndex.Value % (snapshotSize + 1);

            // Load events
            var events = _eventRepository.GetEventsByAggregateId(loadFromEvent, aggregateId);
            if (events == null)
                throw new IndexOutOfRangeException("Could not find Aggregate with ID: " + aggregateId);

            // Event spool aggregate
            var aggregate = _aggregateFactory.Create<T>(
                aggregateId,
                events.Select(x => x.EventPayload).ToArray()
            );

            return aggregate;
        }

        public OrderedEventPayload[] Save(IAggregate aggregate)
        {
            //TODO: Handle errors

            var events = aggregate.GetUncommittedEvents().ToArray();
            if (events.Any() == false)
                return new OrderedEventPayload[]{}; // Nothing to save

            var aggregateType = aggregate.GetType().Name;

            var originalVersion = aggregate.Version - events.Count() + 1;

            var eventsToSave = events
                .Select(e => e.ToEventData(aggregateType, aggregate.Id, originalVersion++))
                .ToArray();

            var storedAggregateVersion = _eventRepository.GetVersionByAggregateId(aggregate.Id);
            if (storedAggregateVersion.HasValue && storedAggregateVersion >= originalVersion)
                throw new Exception("Concurrency exception");

            var orderedEvents = _eventRepository.SaveEvents(eventsToSave);

            aggregate.ClearUncommittedEvents();

            return orderedEvents;
        }
    }
}