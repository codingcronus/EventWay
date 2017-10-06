using System;

namespace EventWay.Query
{
    public abstract class QueryModel
    {
        protected QueryModel(Guid id)
        {
            //AggregateId = aggregateId;
            this.id = id;
        }

        //public Guid AggregateId { get; private set; }
        //public Guid id { get { return AggregateId; } }
        public Guid id { get; set; }
        public string partitionKey { get; set; }
        public string Type => GetType().Name;
    }
}
