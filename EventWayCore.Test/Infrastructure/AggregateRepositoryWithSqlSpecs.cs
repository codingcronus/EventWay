using EventWayCore.Core;
using EventWayCore.Infrastructure;
using EventWayCore.Infrastructure.MsSql;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EventWayCore.Infrastructure.Redis;
using Aggregate = EventWayCore.Core.Aggregate;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class AggregateRepositoryWithRedisSpecs
    {
        [Test]
        public async Task ShouldSuccesfullyPersistAndHydrateEventsFromAggregate()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            //var eventRepository = new SqlServerEventRepository("Server=tcp:shuffle.database.windows.net,1433;Initial Catalog=Shuffle;Persist Security Info=False;User ID=kvinther;Password=k1617v_KV;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Application Name=Shuffle;");
            var eventRepository = new SqlServerEventRepository("Server=.\\sqlexpress;Initial Catalog=VerdunEvents;Persist Security Info=False;Trusted_Connection=True;MultipleActiveResultSets=False;Connection Timeout=30;Application Name=Shuffle;");
            var aggregateFactory = new DefaultAggregateFactory();
            var repository = new AggregateRepository(eventRepository, aggregateFactory);
            var listener = new BasicEventListener();

            IAggregateCache cache = null;
            //cache = new RedisAggregateCache("shuffle.redis.cache.windows.net:6380,password=z0cYd5K7aNjtE9tl8x6nxYa2lK5TwrJcB1aHsZGCx5Q=,ssl=True,abortConnect=False");
            cache = new RedisAggregateCache("localhost");
            var store = new AggregateStore(repository, listener, aggregateCache: cache);

            // ACT
            var now = DateTime.Now;
            for (var i = 1; i <= 50; i++)
            {
                testAggregate.PublishTestEvent($"{i}");
                await store.Save(testAggregate);
                var hydratedTestAggregate = store.GetById<TestAggregate>(testAggregate.Id);
                // ASSERT
                Assert.AreEqual(testAggregate.Id, hydratedTestAggregate.Id);
                Assert.AreEqual(testAggregate.Version, hydratedTestAggregate.Version);
                Assert.AreEqual(testAggregate.State, hydratedTestAggregate.State);
            }


            Debug.WriteLine($"Total time: {(DateTime.Now - now):g}");
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
