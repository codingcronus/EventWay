using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public abstract class Aggregate : IAggregate
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        private readonly List<object> _uncommittedEvents;

        private readonly Dictionary<Type, Action<object>> _commandHandlers;
        private readonly Dictionary<Type, Action<object>> _eventHandlers;

        protected Aggregate(Guid? id = null)
        {
            Id = id ?? CombGuid.Generate();

            _uncommittedEvents = new List<object>();

            _commandHandlers = new Dictionary<Type, Action<object>>();
            _eventHandlers = new Dictionary<Type, Action<object>>();
        }

        protected void OnCommand<T>(Action<T> handler) where T : class
        {
            if (_commandHandlers.ContainsKey(typeof(T)))
                _commandHandlers[typeof(T)] = (e) => handler(e as T);
            else
                _commandHandlers.Add(typeof(T), (e) => handler(e as T));
        }

        protected void OnEvent<T>(Action<T> handler) where T : class
        {
            if (_eventHandlers.ContainsKey(typeof(T)))
                _eventHandlers[typeof(T)] = (e) => handler(e as T);
            else
                _eventHandlers.Add(typeof(T), (e) => handler(e as T));
        }

        public List<object> GetUncommittedEvents()
        {
            return _uncommittedEvents;
        }

        public void ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }

        public void Apply(object @event)
        {
            Version++;

            RedirectToWhen.InvokeEventOptional(this, @event);
        }

        protected void Publish(object @event)
        {
            _uncommittedEvents.Add(@event);

            Apply(@event);
        }

        public void Tell(IDomainCommand command)
        {
            // Get command type and throw error if command has no handler in aggregate
            var commandType = command.GetType();
            if (!_commandHandlers.ContainsKey(commandType))
                throw new MissingMethodException($"Command of type {command.GetType().ToString()}. not handled");

            _commandHandlers[commandType](command);
        }
    }
}