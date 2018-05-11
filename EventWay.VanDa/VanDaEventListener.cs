using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventWay.Core;

namespace EventWay.VanDa
{
    public class VanDaEventListener : BasicEventListener, IExtendedEventListener
    {
        private readonly Dictionary<Type, List<Func<OrderedEventPayload[], Task>>> _eventCollectionHandlers;

        public VanDaEventListener() : base()
        {
            _eventCollectionHandlers = new Dictionary<Type, List<Func<OrderedEventPayload[], Task>>>();
        }

        public void OnEvents<T>(Func<OrderedEventPayload[], Task> handler)
        {
            var eventType = typeof(T);

            if (!_eventCollectionHandlers.ContainsKey(eventType))
                _eventCollectionHandlers.Add(eventType, new List<Func<OrderedEventPayload[], Task>>());

            _eventCollectionHandlers[eventType].Add(handler);
        }

        public async Task Handle(OrderedEventPayload[] @events)
        {
            var groupEvents = @events.GroupBy(p => p.EventPayload.GetType());
            foreach (var groupEvent in groupEvents)
            {
                var eventType = groupEvent.Key;
                var eventPayloads = groupEvent.ToArray();
                if (!_eventCollectionHandlers.ContainsKey(eventType))
                {
                    Console.WriteLine($"Received unhandled event: {eventType.AssemblyQualifiedName}");
                    return;
                }

                foreach (var handler in _eventCollectionHandlers[eventType])
                    await handler(eventPayloads);
            }
        }
    }
}