using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public interface ISnapshotEventRepository
    {
        List<OrderedEventPayload> GetSnapshotEventsByAggregateId(Guid aggregateId);
        object GetSnapshotEventByAggregateIdAndVersion(Guid aggregateId, int version);
        int? GetVersionByAggregateId(Guid aggregateId);
        void SaveSnapshotEvent(Event snapshotEvent);
        void SaveSnapshotEvents(Event[] snapshotEvents);
        void ClearSnapshotEventsByAggregateId(Guid aggregateId, int to);
        void ClearSnapshotEventsByAggregateId(Guid aggregateId);
        void ClearSnapshotEvents();
        
    }
}