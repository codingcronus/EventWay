namespace EventWay.Query
{
    public class PagedQuery
    {
        public string ContinuationToken { get; set; }
        public int MaxItemCount { get; set; }
    }
}
