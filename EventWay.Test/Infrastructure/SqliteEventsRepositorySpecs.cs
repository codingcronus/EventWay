using System;
using System.Data.SQLite;
using System.Linq;
using EventWay.Core;
using EventWay.Infrastructure.Sqlite;
using Dapper;
using NUnit.Framework;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class SqliteEventsRepositorySpecs
    {
        private readonly string _connectionString = @"Data Source=C:\temp\sqlite-test.db;Version=3;";
        private const string InMemConnectionString = @"Data Source =:memory:;Version=3;";
        private readonly Guid _aggregateId;

        public SqliteEventsRepositorySpecs()
        {
            //_connectionString = InMemConnectionString;
            _aggregateId = Guid.Parse("e5cbb67b-c12f-42e0-8a32-a82a012df68a");
            SqliteDbTool.CreateTables(_connectionString);
        }

        [Test]
        public void CanCreateDatabaseTables()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                // Create tables.
                SqliteDbTool.CreateTables(connection);

                var sql = "SELECT Count(*) FROM sqlite_master Where type='table' And name In ('Events', 'SnapshotEvents')";

                var count = connection.ExecuteScalar<int>(sql);

                Assert.AreEqual(2, count);
            }
        }

        [Test]
        public void ShouldSaveEvents()
        {
            // ARRANGE
            var repository = new SqliteEventRepository(_connectionString);

            var testEvent = new SqliteTestEvent
            {
                DummyPayload = "Hello, World!",
            };

            var e = testEvent.ToEventData("TestAggregate", _aggregateId, 1);

            // ACT
            var events = repository.SaveEvents(new[] {e});

            // ASSERT
            Assert.AreEqual(1, events.Length);
            Assert.AreNotEqual(0, events.First().Ordering);
        }

        [Test]
        public void ShouldGetEvents()
        {
            // ARRANGE
            var repository = new SqliteEventRepository(_connectionString);

            // ACT
            var orderedEvents = repository.GetEventsByAggregateId(_aggregateId);
            
            // ASSERT
        }

        [Test]
        public void ShouldClearEvents()
        {
            // ARRANGE
            var repository = new SqliteEventRepository(_connectionString);

            // ACT
            repository.ClearEvents();

            // ASSERT
        }

        [Test]
        public void ShouldSaveMultipleEvents()
        {
            // ARRANGE
            var repository = new SqliteEventRepository(_connectionString);

            var testEvent = new SqliteTestEvent
            {
                DummyPayload = "Hello, World!",
            };

            const int numberOfEvents = 100;

            var eventsToSave = Enumerable.Range(1, numberOfEvents)
                .Select(x => testEvent.ToEventData("TestAggregate", _aggregateId, x))
                .ToArray();

            // ACT
            var events = repository.SaveEvents(eventsToSave);

            // ASSERT
            Assert.AreEqual(numberOfEvents, events.Length);
            Assert.IsFalse(events.Any(x => x.Ordering == 0));
            Assert.AreEqual(numberOfEvents, events.Select(x => x.Ordering).Distinct().Count());
        }

        [Test]
        public void ShouldHandleLongFromDatabaseAsInt()
        {
            try
            {
                var longNumber = 1L;
                var intNumber = (int?) longNumber;
                Assert.AreEqual(longNumber, intNumber);
            }
            catch (InvalidCastException)
            {
                Assert.True(true);
            }
        }

        public class SqliteTestEvent
        {
            public string DummyPayload { get; set; }
        }
    }
}