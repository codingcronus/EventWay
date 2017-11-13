using System;
using System.Linq;
using NUnit.Framework;
using EventWay.Core;
using EventWay.Infrastructure;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class InMemorySnapshotEventsRepositorySpecs
    {
        public class IncrementingAggregate : Aggregate
        {
            private int _value;
            public IncrementingAggregate(Guid id) : base(id)
            {
                OnCommand<Increment>(c => Publish(new Incremented()));
                OnCommand<GetValue, int>(c => _value);
                OnEvent<Incremented>(e => _value++);
                OnEvent<SnapshotOffer>(e => _value = Convert.ToInt32(e.State));
            }

            protected override object GetState() => _value;

            public class Increment : IDomainCommand { }
            public class GetValue : IDomainCommand { }
            public class Incremented : DomainEvent { }
        }

        [Test]
        public void ShouldComposeSnapshotOfExactNumberOfEvents()
        {
            // ARRANGE
            var eventRepository = new InMemoryEventRepository();
            var snapshotRepository = new InMemorySnapshotEventRepository();

            var factory = new DefaultAggregateFactory();
            var aggregateStore = new AggregateRepository(eventRepository, factory, snapshotRepository);

            // Create a test aggregate.
            var aggregateId = CombGuid.Generate();
            var aggregate = aggregateStore.GetById<IncrementingAggregate>(aggregateId);

            var snapshotSize = aggregate.SnapshotSize;
            var numberOfTestEvents = 2 * snapshotSize + 1;
            
            // Send test events to the aggregate.
            Enumerable.Range(1, numberOfTestEvents) 
                .ToList()
                .ForEach(x => aggregate.Tell(new IncrementingAggregate.Increment()));

            // ACT
            var orderedEvents = aggregateStore.Save(aggregate);
            var snapshotVersion = snapshotRepository.GetVersionByAggregateId(aggregateId);
            var snapshotOffer = (SnapshotOffer) snapshotRepository
                .GetSnapshotEventByAggregateIdAndVersion(aggregateId, snapshotVersion ?? 0);
            var newAggregate = aggregateStore.GetById<IncrementingAggregate>(aggregateId);
            var newState = newAggregate.Ask<int>(new IncrementingAggregate.GetValue());

            // ASSERT
            Assert.AreEqual(numberOfTestEvents, orderedEvents.Length);
            Assert.AreEqual(numberOfTestEvents - 1, snapshotOffer.Version);
            Assert.AreEqual(numberOfTestEvents - 1, snapshotOffer.State);
            Assert.AreEqual(numberOfTestEvents, newState);
        }
    }
}