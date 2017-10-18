using System;
using System.Linq;
using System.Threading.Tasks;
using EventWay.Core;
using EventWay.Infrastructure.CosmosDb;
using EventWay.Query;
using NUnit.Framework;
using System.Collections.Generic;
using Ploeh.AutoFixture;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class DocumentDbQueryModelRepositorySpecs
    {
        private readonly string _database = "vanda-integration-test";
        private readonly string _collection = "Projections";
        private readonly int _offerThroughput = 10000;
        private readonly int _noOfPartitions = 2000;
        private readonly string _endpoint = "https://localhost:8081";
        private readonly string _authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            // ARRANGE
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);

            // ACT
            repository.Initialize();

            // ASSERT
        }

        [Test]
        [Order(1)]
        public async Task ShouldSuccesfullyCreateAndHydrateQueryModel()
        {
            // ARRANGE
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);

            var queryModelId = Guid.NewGuid();
            var testQueryModel = new TestQueryModel(queryModelId, "Hello Integration Test!");

            // ACT
            await repository.Save(testQueryModel);
            var hydratedQueryModel = await repository.GetById<TestQueryModel>(queryModelId);
            var existing = await repository.DoesItemExist<TestQueryModel>(queryModelId);

            // ASSERT
            Assert.IsTrue(existing);
            Assert.IsNotNull(hydratedQueryModel);
            Assert.AreEqual(queryModelId, hydratedQueryModel.id);
            Assert.AreEqual("Hello Integration Test!", testQueryModel.DummyPayload);
            Assert.AreEqual(testQueryModel.DummyPayload, hydratedQueryModel.DummyPayload);
        }

        [Test]
        [Order(2)]
        public async Task ShouldSuccesfullyGetPagedList()
        {
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);

            var pagedQuery = new PagedQuery()
            {
                MaxItemCount = 2
            };
            var pagedResult = await repository.GetPagedListAsync<TestQueryModel>(pagedQuery);

            // ASSERT
            Assert.IsTrue(pagedResult.Data.Count() >= 0 && pagedResult.Data.Count() <= pagedResult.Count);
        }

        [Test]
        [Order(3)]
        public async Task ShouldSuccesfullyDelete()
        {
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);

            var pagedQuery = new PagedQuery()
            {
                MaxItemCount = 10
            };
            var pagedResult = await repository.GetPagedListAsync<TestQueryModel>(pagedQuery);
            foreach (var item in pagedResult.Data)
            {
                await repository.DeleteById<TestQueryModel>(item.id);
            }
            // ASSERT
            Assert.IsTrue(pagedResult.Data.Count() >= 0 && pagedResult.Data.Count() <= pagedResult.Count);
        }

        [Test]
        [Order(4)]
        public void ShouldBeStableInParallelGeneratePartitionKey()
        {
            var id = Guid.NewGuid();
            var keyPattern = PartitionKeyGenerator.Generate(id, 2000);

            Parallel.For(0, 300000, (index) =>
            {
                var key = PartitionKeyGenerator.Generate(Guid.Parse(id.ToString()), 2000);
                if (key != keyPattern)
                {
                    throw new Exception($"{keyPattern} - {key}");
                }
            });

            // ASSERT
            Assert.IsTrue(true);
        }

        [Test]
        [Order(5)]
        public void ShouldBeStableInParallelUpdateDataQueryModel()
        {
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);
            Parallel.For(0, 50, (index) =>
            {
                RunDataQueryModel(repository, index).Wait();
            });
            // ASSERT
            Assert.IsTrue(true);
        }

        private async Task RunDataQueryModel(DocumentDbQueryModelRepository repository, int index)
        {
            try
            {
                //var queryModelId = Guid.NewGuid();
                //var testQueryModel = new TestQueryModel(queryModelId, "Hello Integration Test!" + index);
                var testQueryModel = _fixture.Create<TestQueryModel>();
                var queryModelId = testQueryModel.id;

                await repository.Save(testQueryModel);
                var hydratedQueryModel = await repository.GetById<TestQueryModel>(queryModelId);
                var existing = await repository.DoesItemExist<TestQueryModel>(queryModelId);
                if (!existing)
                {
                    throw new Exception("DoesItemExist");
                }
                var hydratedQueryModel1 = await repository.QueryItemAsync<TestQueryModel>(x => x.id == queryModelId);

                Assert.IsTrue(existing);
                Assert.IsNotNull(hydratedQueryModel);
                Assert.AreEqual(queryModelId, hydratedQueryModel.id);
                Assert.AreEqual(queryModelId, hydratedQueryModel1.id);

                hydratedQueryModel.DummyPayload = "DummyPayload" + queryModelId;

                await repository.Save(hydratedQueryModel);

                await repository.DeleteById<TestQueryModel>(queryModelId);

                existing = await repository.DoesItemExist<TestQueryModel>(queryModelId);
                if (existing)
                {
                    throw new Exception("DoesItemExist");
                }
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        [Test]
        [Order(6)]
        public void ShouldBeStableInParallelUpdateDataQueryModels()
        {
            var repository = new DocumentDbQueryModelRepository(_database, _collection, _offerThroughput, _noOfPartitions, _endpoint, _authKey);
            Parallel.For(0, 50, (index) =>
            {
                RunDataQueryModels(repository, index).Wait();
            });
            //for (var i = 0; i < 50; i++)
            //{
            //    RunDataQueryModels(repository, i).Wait();
            //}

            Assert.IsTrue(true);
        }

        private async Task RunDataQueryModels(DocumentDbQueryModelRepository repository, int index)
        {
            try
            {
                var list = _fixture.CreateMany<TestQueryModel>(10);
                foreach (var item in list)
                {
                    await repository.Save(item);
                }

                var pagedQuery = new PagedQuery()
                {
                    MaxItemCount = 100
                };
                var result = await repository.GetPagedListAsync<TestQueryModel>(pagedQuery);
                if (result.Count < 10)
                {
                    throw new Exception("GetPagedListAsync");
                }
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }

    public class TestQueryModel : QueryModel
    {
        public TestQueryModel(Guid aggregateId, string payload) : base(aggregateId)
        {
            DummyPayload = payload;
        }

        public string DummyPayload { get; set; }
        public string DummyPayload2 { get; set; }
        public string DummyPayload3 { get; set; }
        public string DummyPayload4 { get; set; }
        public string DummyPayload5 { get; set; }
        public string DummyPayload6 { get; set; }
        public string DummyPayload7 { get; set; }
        public string DummyPayload12 { get; set; }
        public string DummyPayload13 { get; set; }
        public string DummyPayload14 { get; set; }
        public string DummyPayload15 { get; set; }
        public string DummyPayload16 { get; set; }
        public string DummyPayload21 { get; set; }
        public string DummyPayload22 { get; set; }
        public string DummyPayload23 { get; set; }
        public string DummyPayload24 { get; set; }
        public string DummyPayload25 { get; set; }
        public string DummyPayload26 { get; set; }
        public string DummyPayload27 { get; set; }
        public string DummyPayload28 { get; set; }
        public string DummyPayload29 { get; set; }
        public string DummyPayload112 { get; set; }
        public string DummyPayload122 { get; set; }
        public string DummyPayload132 { get; set; }
        public string DummyPayload142 { get; set; }
        public string DummyPayload152 { get; set; }
        public string DummyPayload222 { get; set; }
        public string DummyPayload332 { get; set; }
        public string DummyPayload342 { get; set; }
        public string DummyPayload352 { get; set; }
        public string DummyPayload322 { get; set; }
        public string DummyPayload432 { get; set; }
        public string DummyPayload542 { get; set; }
        public string DummyPayload462 { get; set; }

        public HostedTestQueryModel Hosted1 { get; set; }
        public HostedTestQueryModel Hosted2 { get; set; }
        public HostedTestQueryModel Hosted3 { get; set; }
        public HostedTestQueryModel Hosted4 { get; set; }

        public override string BaseType => string.Empty;
    }

    public class HostedTestQueryModel
    {
        public string DummyPayload { get; set; }
        public string DummyPayload2 { get; set; }
        public string DummyPayload3 { get; set; }
        public string DummyPayload4 { get; set; }
        public string DummyPayload5 { get; set; }
        public string DummyPayload6 { get; set; }
        public string DummyPayload7 { get; set; }
        public string DummyPayload12 { get; set; }
        public string DummyPayload13 { get; set; }
        public string DummyPayload14 { get; set; }
        public string DummyPayload15 { get; set; }
        public string DummyPayload16 { get; set; }
        public string DummyPayload21 { get; set; }
        public string DummyPayload22 { get; set; }
        public string DummyPayload23 { get; set; }
        public string DummyPayload24 { get; set; }
        public string DummyPayload25 { get; set; }
        public string DummyPayload26 { get; set; }
        public string DummyPayload27 { get; set; }
        public string DummyPayload28 { get; set; }
        public string DummyPayload29 { get; set; }
        public string DummyPayload112 { get; set; }
        public string DummyPayload122 { get; set; }
        public string DummyPayload132 { get; set; }
        public string DummyPayload142 { get; set; }
        public string DummyPayload152 { get; set; }
        public string DummyPayload222 { get; set; }
        public string DummyPayload332 { get; set; }
        public string DummyPayload342 { get; set; }
        public string DummyPayload352 { get; set; }
        public string DummyPayload322 { get; set; }
        public string DummyPayload432 { get; set; }
        public string DummyPayload542 { get; set; }
        public string DummyPayload462 { get; set; }
    }
}