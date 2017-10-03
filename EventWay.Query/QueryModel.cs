using System;

namespace EventWay.Query
{
    public abstract class QueryModel
    {
        protected QueryModel(Guid aggregateId)
        {
            AggregateId = aggregateId.ToString();
        }

        public string AggregateId { get; set; } //TODO: Setter should be private, but in that case DocumentDB can't hydrate it

        // Avoid to update by AutoMapper
        public string id { get { return Type + "-" + AggregateId; } }
        public string partitionKey { get; set; }
        public string Type => GetType().Name;
    }
}
