using System.Collections.Generic;

namespace EventWay.Core
{
    public interface IAggregateTracking
    {
        void TrackEvents(IAggregate aggregate);
    }
}
