using System;
using EventWay.Core;
using EventWay.Infrastructure.MsSql;
using NUnit.Framework;
namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class SqlServerEventsRepositorySpecs
    {
        [Test]
        public void ShouldRecognizeEventTypeFromFullyQualifiedAssemblyName()
        {
            // ARRANGE
            var @event = new SqlServerEventsRepositorySpecsTestEvent();
            var eventTypeName = @event.GetType().AssemblyQualifiedName;

            // ACT
            var eventType = Type.GetType(eventTypeName);

            // ASSERT
            Assert.NotNull(eventType);
        }

        [Test]
        public void ShouldSuccesfullyPersistAndHydrateEventForAggregate()
        {
            // ARRANGE
            var connectionString = "Data Source=localhost;Initial Catalog=vanda-db-dev;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var repository = new SqlServerEventRepository(connectionString);

            var testEventPayload = new SqlServerEventsRepositorySpecsTestEvent()
            {
                DummyPayload = "Integration Test " + CombGuid.Generate()
            };

            var aggregateId = CombGuid.Generate();

            var testEvent = testEventPayload.ToEventData("TestAggregate", aggregateId, 1);

            // ACT
            repository.SaveEvents(new []{ testEvent });

            var hydratedEvents = repository.GetEventsByAggregateId(aggregateId);

            // ASSERT
            Assert.AreEqual(1, hydratedEvents.Count);
            Assert.AreEqual(typeof(SqlServerEventsRepositorySpecsTestEvent), hydratedEvents[0].EventPayload.GetType());
        }
    }

    public class SqlServerEventsRepositorySpecsTestEvent
    {
        public string DummyPayload { get; set; }
    }
}
