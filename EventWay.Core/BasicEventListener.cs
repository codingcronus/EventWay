using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventWay.Core
{
	public class BasicEventListener : IEventListener
	{
		private readonly Dictionary<Type, List<Func<OrderedEventPayload, Task>>> _eventHandlers;

		public BasicEventListener()
		{
			_eventHandlers = new Dictionary<Type, List<Func<OrderedEventPayload, Task>>>();
		}

		public void OnEvent<T>(Func<OrderedEventPayload, Task> handler)
		{
			var eventType = typeof(T);

			if (!_eventHandlers.ContainsKey(eventType))
				_eventHandlers.Add(eventType, new List<Func<OrderedEventPayload, Task>>());

			_eventHandlers[eventType].Add(handler);
		}

		public async Task Handle(OrderedEventPayload @event)
		{
			var eventType = @event.EventPayload.GetType();

            if (!_eventHandlers.ContainsKey(eventType))
                return;

			foreach (var handler in _eventHandlers[eventType])
				await handler(@event);
		}
	}
}
