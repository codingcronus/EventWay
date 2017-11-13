using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EventWay.Core;

namespace EventWay.Infrastructure
{
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly IDictionary<int, Event> _inMemoryStorage;

        public InMemoryEventRepository()
        {
            _inMemoryStorage = new ConcurrentDictionary<int, Event>();
        }

        public List<OrderedEventPayload> GetEvents<TAggregate>(long @from) where TAggregate : Aggregate
        {
            return _inMemoryStorage
                .Where(x => x.Key > from)
                .Select(x => x.Value.DeserializeOrderedEvent())
                .ToList();
        }

        public List<OrderedEventPayload> GetEventsByAggregateId(Guid aggregateId)
        {
            return GetEventsByAggregateId(0, aggregateId);
        }

        public List<OrderedEventPayload> GetEventsByAggregateId(long @from, Guid aggregateId)
        {
            return _inMemoryStorage
                .Where(x => x.Key > from)
                .Where(x => x.Value.AggregateId == aggregateId)
                .Select(x => x.Value.DeserializeOrderedEvent())
                .ToList();
        }

        public List<OrderedEventPayload> GetEventsByType(Type eventType)
        {
            return GetEventsByTypes(0, new[] { eventType });
        }

        public List<OrderedEventPayload> GetEventsByType(long @from, Type eventType)
        {
            return GetEventsByTypes(from, new[] {eventType});
        }

        public List<OrderedEventPayload> GetEventsByTypes(Type[] eventTypes)
        {
            return GetEventsByTypes(0, eventTypes);
        }

        public List<OrderedEventPayload> GetEventsByTypes(long @from, Type[] eventTypes)
        {
            var eventTypeNames = eventTypes
                .Select(x => x.Name)
                .ToArray();

            return _inMemoryStorage
                .Where(x => x.Key > from)
                .Where(x => eventTypeNames.Contains(x.Value.EventType))
                .Select(x => x.Value.DeserializeOrderedEvent())
                .OrderBy(x => x.Ordering)
                .ToList();
        }

        public int? GetVersionByAggregateId(Guid aggregateId)
        {

            var aggregateEvents = _inMemoryStorage
                .Where(x => x.Value.AggregateId == aggregateId)
                .ToArray();

            if (aggregateEvents.Any())
                return aggregateEvents.Max(x => x.Value.Version);
            return null;
        }

        public OrderedEventPayload[] SaveEvents(Event[] eventsToSave)
        {
            var addedEvents = new List<Event>();
            var nextOrdering = _inMemoryStorage.Any()
                ? _inMemoryStorage.Max(x => x.Key) + 1
                : 1;
            var orderedEvents = eventsToSave
                .OrderBy(x => x.AggregateId)
                .ThenBy(x => x.Version);
            foreach (var @event in orderedEvents)
            {
                var e = new Event
                {
                    Ordering = nextOrdering,
                    Version = @event.Version,
                    AggregateId = @event.AggregateId,
                    AggregateType = @event.AggregateType,
                    Created = @event.Created,
                    EventId = @event.EventId,
                    EventType = @event.EventType,
                    Metadata = @event.Metadata,
                    Payload = @event.Payload
                };
                addedEvents.Add(e);
                _inMemoryStorage[nextOrdering++] = e;
            }

            return addedEvents
                .Select(x => x.DeserializeOrderedEvent())
                .ToArray();
        }

        public void ClearEvents()
        {
            _inMemoryStorage.Clear();
        }
    }
}