using System;

namespace EventWayCore.Core
{
    public class Event
    {
        public long Ordering { get; set; }
        public Guid EventId { get; set; }
        public DateTime Created { get; set; }
        public string EventType { get; set; }
        public string AggregateType { get; set; }
        public Guid AggregateId { get; set; }
        public int Version { get; set; }
        public string Payload { get; set; }
        public string Metadata { get; set; }
    }
}
