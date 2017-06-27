using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public interface IAggregate
    {
        Guid Id { get; set; }
        int Version { get; set; }

        List<object> GetUncommittedEvents();
        void ClearUncommittedEvents();

        void Apply(object @event);
    }
}
