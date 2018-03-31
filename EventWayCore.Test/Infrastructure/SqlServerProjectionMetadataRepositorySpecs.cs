using EventWayCore.Core;
using EventWayCore.Infrastructure.MsSql;
using EventWayCore.Query;
using NUnit.Framework;

namespace EventWay.Test.Infrastructure
{
    [TestFixture(Category = "Integration")]
    public class SqlServerProjectionMetadataRepositorySpecs
    {
        private readonly string _connectionString = "Data Source=localhost;Initial Catalog=vanda-db-dev;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        [Test]
        public void ShouldSuccesfullyCreateAndHydrateProjectionMetadata()
        {
            // ARRANGE
            var repository = new SqlServerProjectionMetadataRepository(_connectionString);

            var projectionId = CombGuid.Generate();
            var projectionType = "TestProjection";

            // ACT
            repository.InitializeProjection(projectionId, projectionType);

            var hydratedProjectionMetadata = repository.GetByProjectionId(projectionId);

            // ASSERT
            Assert.AreEqual(projectionId, hydratedProjectionMetadata.ProjectionId);
            Assert.AreEqual(0L, hydratedProjectionMetadata.EventOffset);
        }

        [Test]
        public void ShouldSuccesfullyUpdateAndHydrateProjectionMetadata()
        {
            // ARRANGE
            var repository = new SqlServerProjectionMetadataRepository(_connectionString);

            var projectionId = CombGuid.Generate();
            var projectionType = "TestProjection";

            // ACT
            repository.InitializeProjection(projectionId, projectionType);
            repository.UpdateEventOffset(new ProjectionMetadata(projectionId, 1));

            var hydratedProjectionMetadata = repository.GetByProjectionId(projectionId);

            // ASSERT
            Assert.AreEqual(projectionId, hydratedProjectionMetadata.ProjectionId);
            Assert.AreEqual(1L, hydratedProjectionMetadata.EventOffset);
        }
    }
}
