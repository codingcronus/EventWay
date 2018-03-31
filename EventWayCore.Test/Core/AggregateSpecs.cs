using EventWayCore.Core;
using NUnit.Framework;
using System;

namespace EventWay.Test.Core
{
    [TestFixture(Category = "Unit")]
    public class AggregateSpecs
    {
        public static bool WhenMethodInvoked;

        [Test]
        public void ShouldSuccesfullyInvokeTheAggregatesWhenMethod()
        {
            // ARRANGE
            var testAggregate = new UnitTestAggregate(Guid.NewGuid());

            // ACT
            testAggregate.PublishTestEvent();

            // ASSERT
            Assert.IsTrue(WhenMethodInvoked);
        }


        // INTERNAL TEST CLASSES
        public class TestEvent
        {
            public string DummyPayload { get; set; }
        }

        public class UnitTestAggregate : Aggregate
        {
            public UnitTestAggregate(Guid id) : base(id)
            {
                OnEvent<TestEvent>(When);
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
                AggregateSpecs.WhenMethodInvoked = true;
            }
        }
    }
}
