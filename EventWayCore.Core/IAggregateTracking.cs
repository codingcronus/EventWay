using System.Collections.Generic;

namespace EventWayCore.Core
{
    public interface IAggregateTracking
    {
        void TrackEvents(IAggregate aggregate);
        void TrackEvents<T>(IEnumerable<T> aggregates) where T : IAggregate;
    }
}
