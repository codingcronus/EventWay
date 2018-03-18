using EventWay.Core;
using EventWay.Infrastructure;
using EventWay.Infrastructure.MsSql;
using NUnit.Framework;
using System;
using EventWay.Infrastructure.InMemory;
using EventWay.Infrastructure.Redis;
using StackExchange.Redis;
using Aggregate = EventWay.Core.Aggregate;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class AggregateRepositoryWithRedisSpecs
    {
        [Test]
        public void ShouldSuccesfullyPersistAndHydrateEventsFromAggregate()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            var eventRepository = new SqlServerEventRepository();
            var aggregateFactory = new DefaultAggregateFactory();
            var repository = new AggregateRepository(eventRepository, aggregateFactory);
            var listener = new BasicEventListener();

            var cache = new RedisAggregateCache("localhost");
            var store = new AggregateStore(repository, listener, aggregateCache: cache);

            // ACT
            testAggregate.PublishTestEvent();
            testAggregate.PublishTestEvent("How are you doing?");

            store.Save(testAggregate).GetAwaiter().GetResult();

            var hydratedTestAggregate = store.GetById<TestAggregate>(testAggregate.Id);

            

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
            public TestAggregate(Guid id) : base(id) 
                => OnEvent<TestEvent>(When);

            public void PublishTestEvent(string payload = "Hello World")
            {
                var testEvent = new TestEvent()
                {
                    DummyPayload = payload
                };

                Publish(testEvent);
            }

            private void When(TestEvent @event)
            {
                var newState = string.IsNullOrEmpty(State)
                    ? @event.DummyPayload
                    : $"{State}:{@event.DummyPayload}";
                State = newState;
            }

            public string State { get; private set; }
        }
    }

    [TestFixture(Category = "Integration")]
    public class AggregateRepositoryWithSqlSpecs
    {
        [Test]
        public void ShouldSuccesfullyPersistAndHydrateEventsFromAggregate()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

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
            public TestAggregate(Guid id) : base(id)
            {

            }

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
