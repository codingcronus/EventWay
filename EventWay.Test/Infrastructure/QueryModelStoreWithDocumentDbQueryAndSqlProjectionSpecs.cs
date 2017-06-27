using System.Threading.Tasks;
using EventWay.Core;
using EventWay.Infrastructure.MsSql;
using EventWay.Infrastructure.CosmosDb;
using EventWay.Query;
using NUnit.Framework;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class QueryModelStoreWithDocumentDbQueryAndSqlProjectionSpecs
    {
        // DocumentDB
        private readonly string _database = "vanda-integration-test";
        private readonly string _collection = "Projections";
        private readonly string _endpoint = "https://localhost:8081";
        private readonly string _authKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        // MSSQL
        private readonly string _sqlConnectionString = "Data Source=localhost;Initial Catalog=vanda-db-dev;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        [Test]
        public async Task ShouldSuccesfullyInitializeDatabaseAndCollection()
        {
            // ARRANGE
            var eventOffset = 0L;
            var projectionId = CombGuid.Generate();
            var projectionType = "IntegrationTestProjection";

            // Query Model Repository (DocumentDB)
            var queryModelRepository = new DocumentDbQueryModelRepository(_database, _collection, _endpoint, _authKey);
            queryModelRepository.Initialize();

            // Projection Metadata Repository (MSSQL);
            var projectionMetadataRepository = new SqlServerProjectionMetadataRepository(_sqlConnectionString, createProjectionMetadataTable: true);
            projectionMetadataRepository.InitializeProjection(projectionId, projectionType);

            // Dummy Query Model
            var queryModelId = CombGuid.Generate();
            var testQueryModel = new TestQueryModel(queryModelId, "Hello Integration Test!");

            // Query Model Store
            var queryModelStore = new QueryModelStore(
                queryModelRepository, 
                projectionMetadataRepository,
                eventOffset,
                projectionId);
            
            // ACT
            await queryModelStore.SaveQueryModel(testQueryModel);

            // ASSERT
        }
    }
}
