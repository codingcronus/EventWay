using EventWay.Core;
using NUnit.Framework;

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
            var testAggregate = new TestAggregate();

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
                AggregateSpecs.WhenMethodInvoked = true;
            }
        }
    }
}
