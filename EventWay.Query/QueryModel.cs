using System;

namespace EventWay.Query
{
    public abstract class QueryModel
    {
        protected QueryModel(Guid aggregateId)
        {
            AggregateId = aggregateId.ToString();
            id = GetType().Name + "-" + aggregateId;
        }

        public string AggregateId { get; set; } //TODO: Setter should be private, but in that case DocumentDB can't hydrate it

        // ReSharper disable once InconsistentNaming
        public string id { get; protected set; }
        public string Type => GetType().Name;
    }
}
