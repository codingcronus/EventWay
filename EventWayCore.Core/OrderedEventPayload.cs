namespace EventWayCore.Core
{
    public class OrderedEventPayload
    {
        public OrderedEventPayload(long ordering, object eventPayload)
        {
            Ordering = ordering;
            EventPayload = eventPayload;
        }

        public long Ordering { get; private set; }
        public object EventPayload { get; private set; }
    }
}