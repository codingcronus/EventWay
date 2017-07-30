using System;
using System.Collections.Generic;

namespace EventWay.Core
{
    public interface IAggregate
    {
        int SnapshotSize { get; }

        Guid Id { get; set; }
        int Version { get; set; }

        List<object> GetUncommittedEvents();
        void ClearUncommittedEvents();

        void Apply(object @event);
    }
}
