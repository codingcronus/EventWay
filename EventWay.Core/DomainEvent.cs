using System;

namespace EventWay.Core
{
    public abstract class DomainEvent
    {
        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }
    }
}