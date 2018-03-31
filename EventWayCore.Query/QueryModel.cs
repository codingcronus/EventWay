using System;

namespace EventWayCore.Query
{
    public abstract class QueryModel
    {
        protected QueryModel(Guid aggregateId)
        {
            id = Guid.NewGuid();

            AggregateId = aggregateId;
        }

        public Guid id { get; set; }

        public Guid AggregateId { get; set; }
        public string Type => GetType().Name;
    }
}
