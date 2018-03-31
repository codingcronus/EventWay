using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventWayCore.Core
{
    public static class ExtendsEvent
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new ShouldSerializeContractResolver()
        };

        public static OrderedEventPayload DeserializeOrderedEvent(this Event x)
        {
            return new OrderedEventPayload(x.Version, x.DeserializeEvent());
        }

        public static object DeserializeEvent(this Event x)
        {
            var eventType = Type.GetType(x.EventType);
            var deserializedPayload = JsonConvert.DeserializeObject(x.Payload, eventType, SerializerSettings);

            if (deserializedPayload.GetType().IsSubclassOf(typeof(DomainEvent)))
            {
                ((DomainEvent) deserializedPayload).AggregateId = x.AggregateId;
                ((DomainEvent) deserializedPayload).AggregateType = x.AggregateType;
            }

            return deserializedPayload;
        }

        public static Event ToEventData(this object @event, string aggregateType, Guid aggregateId, int version)
        {
            /*var eventHeaders = new Dictionary<string, object>
            {
                {
                    "EventClrType", @event.GetType().AssemblyQualifiedName
                }
            };
            var metadata = JsonConvert.SerializeObject(eventHeaders, SerializerSettings);
            */

            string metadata = null;

            var data = JsonConvert.SerializeObject(@event, SerializerSettings);
            var eventId = CombGuid.Generate();

            // TODO: Do we really need AssemblyQualifiedName?
            //var eventType = @event.GetType().AssemblyQualifiedName;
            var eventType = @event.GetType().Name;

            return new Event
            {
                EventId = eventId,
                Created = DateTime.UtcNow,
                EventType = eventType,
                AggregateType = aggregateType,
                AggregateId = aggregateId,
                Version = version,
                Payload = data,
                Metadata = metadata,
            };
        }

        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.ShouldSerialize = instance => true;

                if (property.PropertyName == "AggregateId" ||
                    property.PropertyName == "AggregateType")
                    property.ShouldSerialize = instance => false;

                return property;
            }
        }
    }
}