using System;

namespace EventWay.Query
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

        // Factory method
        public static TQueryModel CreateQueryModel<TQueryModel>(Guid id)
        {
            return (TQueryModel)Activator.CreateInstance(typeof(TQueryModel), id);
        }
    }
}
