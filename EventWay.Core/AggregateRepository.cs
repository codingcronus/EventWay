using System;
using System.Collections.Generic;
using System.Linq;

namespace EventWay.Core
{
    public class AggregateRepository : IAggregateRepository
    {
        private readonly IEventRepository _eventRepository;
        private readonly IAggregateFactory _aggregateFactory;
        private readonly ISnapshotEventRepository _snapshotEventRepository;

        public AggregateRepository(
            IEventRepository eventRepository,
            IAggregateFactory aggregateFactory,
            ISnapshotEventRepository snapshotEventRepository = null)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _aggregateFactory = aggregateFactory ?? throw new ArgumentNullException(nameof(aggregateFactory));

            _snapshotEventRepository = snapshotEventRepository;
        }

        public T GetById<T>(Guid aggregateId) where T : IAggregate
        {
            var loadFromVersion = 0L;
            var eventPayloads = new List<object>();

            // Handle snapshot events.
            var snapshotVersion = _snapshotEventRepository?.GetVersionByAggregateId(aggregateId);
            if (snapshotVersion.HasValue)
            {
                // If a snapshot exists, make that the first event for the recovery of the 
                // aggregate and update the "load from" version.
                var snapshotEvent =
                    _snapshotEventRepository
                        .GetSnapshotEventByAggregateIdAndVersion(aggregateId, snapshotVersion.Value);
                eventPayloads.Add(snapshotEvent);
                loadFromVersion = snapshotVersion.Value;
            }

            // Load events.
            var events = _eventRepository.GetEventsByAggregateId(loadFromVersion, aggregateId);
            if (events == null)
                throw new IndexOutOfRangeException("Could not find Aggregate with ID: " + aggregateId);
            eventPayloads.AddRange(events.Select(x => x.EventPayload));

            // Event spool aggregate.
            aggregate = _aggregateFactory.Create<T>(
                aggregateId,
                eventPayloads.ToArray());

            // Update the aggregate version.
            var aggregateVersion = _eventRepository.GetVersionByAggregateId(aggregateId);
            aggregate.Version = aggregateVersion ?? 0;

            return aggregate;
        }

        public OrderedEventPayload[] Save(IAggregate aggregate)
        {
            return Save(new[] { aggregate });
        }

        public OrderedEventPayload[] Save<T>(IEnumerable<T> aggregates) where T : IAggregate
        {
            var allEventsToSave = new List<Event>();
            var allSnapshotsToSave = new List<Event>();
            foreach (var aggregate in aggregates)
            {
                ExtractEvents(aggregate, 
                    out var eventsToSave, 
                    out var snapshotsToSave, 
                    out var version);
                var storedAggregateVersion = _eventRepository.GetVersionByAggregateId(aggregate.Id);
                if (storedAggregateVersion.HasValue && storedAggregateVersion >= version)
                    throw new Exception($"Concurrency error for aggregate {aggregate.Id}");
                allEventsToSave.AddRange(eventsToSave);
                allSnapshotsToSave.AddRange(snapshotsToSave);
                aggregate.ClearUncommittedEvents();
            }

            _snapshotEventRepository.SaveSnapshotEvents(allSnapshotsToSave.ToArray());

            return _eventRepository.SaveEvents(allEventsToSave.ToArray());
        }

        private static void ExtractEvents(IAggregate aggregate, out List<Event> eventsToSave, out List<Event> snapshotsToSave, out int version)
        {
            eventsToSave = new List<Event>();
            snapshotsToSave = new List<Event>();

            var allEvents = aggregate
                .GetUncommittedEvents()
                .ToArray();

            var numberOfNonSnapshotEvents = allEvents
                .Count(x => !(x is SnapshotOffer));

            var aggregateType = aggregate.GetType().Name;
            version = aggregate.Version - numberOfNonSnapshotEvents + 1;

            foreach (var @event in allEvents)
            {
                // A snapshot offer has the same "version" as the last events it covers.
                // So we do not increment the version.
                if (@event is SnapshotOffer offer)
                    snapshotsToSave.Add(@event.ToEventData(aggregateType, aggregate.Id, offer.Version));
                else
                    eventsToSave.Add(@event.ToEventData(aggregateType, aggregate.Id, version++));
            }
        }
    }
}