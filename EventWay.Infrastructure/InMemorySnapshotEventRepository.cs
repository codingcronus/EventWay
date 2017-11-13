using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EventWay.Core;

namespace EventWay.Infrastructure
{
    public class InMemorySnapshotEventRepository : ISnapshotEventRepository
    {
        private readonly ConcurrentDictionary<int, Event> _inMemoryStorage;

        public InMemorySnapshotEventRepository()
        {
            _inMemoryStorage = new ConcurrentDictionary<int, Event>();
        }

        public List<OrderedEventPayload> GetSnapshotEventsByAggregateId(Guid aggregateId)
        {
            return _inMemoryStorage
                .Where(x => x.Value.AggregateId == aggregateId)
                .Select(x => x.Value.DeserializeOrderedEvent())
                .ToList();
        }

        public object GetSnapshotEventByAggregateIdAndVersion(Guid aggregateId, int version)
        {
            return _inMemoryStorage
                .Where(x => x.Value.AggregateId == aggregateId)
                .Where(x => x.Value.Version == version)
                .Select(x => x.Value.DeserializeEvent())
                .SingleOrDefault();
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

        public void SaveSnapshotEvent(Event snapshotEvent)
        {
            SaveSnapshotEvents(new []{snapshotEvent});
        }

        public void SaveSnapshotEvents(Event[] snapshotEvents)
        {
            var nextOrdering = _inMemoryStorage.Any()
                ? _inMemoryStorage.Max(x => x.Key) + 1
                : 1;

            var orderedEvents = snapshotEvents
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
                _inMemoryStorage[nextOrdering++] = e;
            }
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId, int to)
        {
            throw new NotImplementedException();
        }

        public void ClearSnapshotEventsByAggregateId(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public void ClearSnapshotEvents()
        {
            throw new NotImplementedException();
        }
    }
}