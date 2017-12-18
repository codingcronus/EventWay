using System;

namespace EventWay.Query
{
    public abstract class QueryModel
    {
        protected QueryModel(Guid id)
        {
            this.id = id;
        }

        public Guid id { get; set; }
        public string Type => GetType().Name;
        public abstract string BaseType { get; }
    }
}
