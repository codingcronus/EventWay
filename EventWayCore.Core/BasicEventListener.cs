using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventWayCore.Core
{
	public class BasicEventListener : IEventListener
	{
		private readonly Dictionary<Type, List<Func<OrderedEventPayload, Task>>> _eventHandlers;
		private readonly Dictionary<Type, List<Func<OrderedEventPayload[], Task>>> _eventCollectionHandlers;

		public BasicEventListener()
		{
			_eventHandlers = new Dictionary<Type, List<Func<OrderedEventPayload, Task>>>();
			_eventCollectionHandlers = new Dictionary<Type, List<Func<OrderedEventPayload[], Task>>>();
		}

		public void OnEvent<T>(Func<OrderedEventPayload, Task> handler)
		{
			var eventType = typeof(T);

			if (!_eventHandlers.ContainsKey(eventType))
				_eventHandlers.Add(eventType, new List<Func<OrderedEventPayload, Task>>());

			_eventHandlers[eventType].Add(handler);
		}

		public void OnEvents<T>(Func<OrderedEventPayload[], Task> handler)
		{
			var eventType = typeof(T);

			if (!_eventCollectionHandlers.ContainsKey(eventType))
				_eventCollectionHandlers.Add(eventType, new List<Func<OrderedEventPayload[], Task>>());

			_eventCollectionHandlers[eventType].Add(handler);
		}

		public async Task Handle(OrderedEventPayload @event)
		{
			var eventType = @event.EventPayload.GetType();

            if (!_eventHandlers.ContainsKey(eventType))
                return;

			foreach (var handler in _eventHandlers[eventType])
				await handler(@event);
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
