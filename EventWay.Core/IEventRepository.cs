using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public interface IEventRepository
    {
        List<OrderedEventPayload> GetEvents();
        List<OrderedEventPayload> GetEvents(long from);

        List<OrderedEventPayload> GetEventsByAggregateId(Guid aggregateId);
        List<OrderedEventPayload> GetEventsByAggregateId(long from, Guid aggregateId);

        List<OrderedEventPayload> GetEventsByType(Type eventType);
        List<OrderedEventPayload> GetEventsByType(long from, Type eventType);

        List<OrderedEventPayload> GetEventsByTypes(Type[] eventTypes);
        List<OrderedEventPayload> GetEventsByTypes(long from, Type[] eventTypes);

        int? GetVersionByAggregateId(Guid aggregateId);

        OrderedEventPayload[] SaveEvents(Event[] eventsToSave);

        void ClearEvents();
    }
}