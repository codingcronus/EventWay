using System;
using System.Threading.Tasks;
using EventWay.Core;

namespace EventWay.VanDa
{
    public interface IExtendedEventListener : IEventListener
    {
        void OnEvents<T>(Func<OrderedEventPayload[], Task> handler);
        Task Handle(OrderedEventPayload[] @events);
    }
}