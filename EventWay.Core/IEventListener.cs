using System;
using System.Threading.Tasks;

namespace EventWay.Core
{
    public interface IEventListener
    {
        void OnEvent<T>(Func<OrderedEventPayload, Task> handler);
        Task Handle(OrderedEventPayload @event);
    }
}