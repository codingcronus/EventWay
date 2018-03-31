using EventWayCore.Core;
using NUnit.Framework;
using System;

namespace EventWay.Test.Core
{
    [TestFixture(Category = "Unit")]
    public class SnapshotsSpecs
    {
        [Test]
        public void CalculateLastSnapshotIndex()
        {
            // ARRANGE
            var numEvents = 16;
            var snapshotSize = 5;

            // ACT
            var snapshotIndex = numEvents - numEvents % (snapshotSize + 1);

            // ASSERT
            Assert.AreEqual(12, snapshotIndex);
        }

        [Test]
        public void ShouldNotSaveSnapshot()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            // ACT
            // Publish 3 events. This should not create a snapshot
            testAggregate.PublishInitialTestEvent();

            // ASSERT
            Assert.AreEqual(3, testAggregate.GetUncommittedEvents().Count);
            Assert.AreEqual("Tigerdyret", testAggregate.State.Name);
            Assert.AreEqual(33, testAggregate.State.Age);
            Assert.AreEqual("my@email.com", testAggregate.State.Email);
        }

        [Test]
        public void ShouldSaveOneSnapshot()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            // ACT
            // Publish 4 events. This should create a snapshot
            testAggregate.PublishInitialTestEvent();
            testAggregate.PublishAddressTestEvent();

            // ASSERT
            Assert.AreEqual(5, testAggregate.GetUncommittedEvents().Count);
            Assert.AreEqual("Tigerdyret", testAggregate.State.Name);
            Assert.AreEqual(33, testAggregate.State.Age);
            Assert.AreEqual("my@email.com", testAggregate.State.Email);
            Assert.AreEqual("Hundred Acre Wood", testAggregate.State.Address);
        }

        [Test]
        public void ShouldNotSaveSnapshotWithOneExistingSnapshot()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            // ACT
            // Publish 7 events. This should create a snapshot
            testAggregate.PublishInitialTestEvent();
            testAggregate.PublishAddressTestEvent();
            testAggregate.PublishInitialTestEvent();

            // ASSERT
            Assert.AreEqual(8, testAggregate.GetUncommittedEvents().Count);
        }

        [Test]
        public void ShouldSaveSnapshotWithOneExistingSnapshot()
        {
            // ARRANGE
            var testAggregate = new TestAggregate(Guid.NewGuid());

            // ACT
            // Publish 10 events. This should create two snapshots
            testAggregate.PublishInitialTestEvent();
            testAggregate.PublishAddressTestEvent();
            testAggregate.PublishInitialTestEvent();
            testAggregate.PublishInitialTestEvent();

            // ASSERT
            Assert.AreEqual(12, testAggregate.GetUncommittedEvents().Count);
        }


        // INTERNAL TEST CLASSES
        public class NameUpdated
        {
            public string Name { get; set; }
        }

        public class AgeUpdated
        {
            public int Age { get; set; }
        }

        public class EmailUpdated
        {
            public string Email { get; set; }
        }

        public class AddressUpdated
        {
            public string Address { get; set; }
        }

        public class TestState
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
        }

        public class TestAggregate : Aggregate
        {
            public override int SnapshotSize => 4;

            public void PublishInitialTestEvent()
            {
                Publish(new NameUpdated() { Name = "Tigerdyret" });
                Publish(new AgeUpdated() { Age = 33 });
                Publish(new EmailUpdated() { Email = "my@email.com" });
            }

            public void PublishAddressTestEvent()
            {
                Publish(new AddressUpdated() { Address = "Hundred Acre Wood" });
            }

            protected override object GetState()
            {
                return this.State;
            }

            public TestState State;

            public TestAggregate(Guid id) : base(id)
            {
                State = new TestState();

                // EVENTS
                OnEvent<NameUpdated>(e =>
                {
                    State.Name = e.Name;
                });

                OnEvent<AgeUpdated>(e =>
                {
                    State.Age = e.Age;
                });

                OnEvent<EmailUpdated>(e =>
                {
                    State.Email = e.Email;
                });

                OnEvent<AddressUpdated>(e =>
                {
                    State.Address = e.Address;
                });

                OnEvent<SnapshotOffer>(e =>
                {
                    var state = (TestState)e.State;

                    State = new TestState()
                    {
                        Name = state.Name,
                        Address = state.Address,
                        Age = state.Age,
                        Email = state.Email
                    };
                });
            }
        }
    }
}