using EventWay.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace EventWay.Infrastructure.ApplicationInsights
{
    public class AggregateTracking : IAggregateTracking
    {
        private readonly TelemetryClient _telemetryClient;

        public AggregateTracking()
        {
            _telemetryClient = new TelemetryClient();
        }

        public void TrackEvents(IAggregate aggregate)
        {
            var evt = new EventTelemetry
            {
                Name = aggregate.GetType().Name
            };

            foreach (var @event in aggregate.GetUncommittedEvents())
            {
                evt.Properties.Add("EventType", @event.GetType().Name);
            }

            _telemetryClient.TrackEvent(evt);
        }
    }
}
