using EventWay.Core;
using EventWay.Infrastructure;
using EventWay.Infrastructure.MsSql;
using NUnit.Framework;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class AggregateRepositoryWithSqlSpecs
    {
        [Test]
        public void ShouldSuccesfullyPersistAndHydrateEventsFromAggregate()
        {
            // ARRANGE
            var testAggregate = new TestAggregate();

            var connectionString = "Data Source=localhost;Initial Catalog=vanda-db-dev;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var sqlEventsRepository = new SqlServerEventRepository(connectionString);

            var aggregateFactory = new DefaultAggregateFactory();

            var repository = new AggregateRepository(sqlEventsRepository, aggregateFactory);

            // ACT
            testAggregate.PublishTestEvent();
            repository.Save(testAggregate);

            var hydratedTestAggregate = repository.GetById<TestAggregate>(testAggregate.Id);

            // ASSERT
            Assert.AreEqual(testAggregate.Id, hydratedTestAggregate.Id);
            Assert.AreEqual(testAggregate.Version, hydratedTestAggregate.Version);
            Assert.AreEqual(testAggregate.State, hydratedTestAggregate.State);
        }

        public class TestEvent
        {
            public string DummyPayload { get; set; }
        }

        public class TestAggregate : Aggregate
        {
            public void PublishTestEvent()
            {
                var testEvent = new TestEvent()
                {
                    DummyPayload = "Hello World"
                };

                this.Publish(testEvent);
            }

            private void When(TestEvent @event)
            {
                State = @event.DummyPayload;
            }

            public string State { get; private set; }
        }
    }
}
