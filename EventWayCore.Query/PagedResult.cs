using System.Collections.Generic;

namespace EventWayCore.Query
{
    public class PagedResult<T> where T : QueryModel
    {
        public int Count { get; private set; }
        public string ContinuationToken { get; private set; }
        public IEnumerable<T> Data { get; private set; }
        

        public PagedResult(IEnumerable<T> data, int count, string continuationToken)
        {
            Count = count;
            Data = data;
            ContinuationToken = continuationToken;
        }
    }
}
