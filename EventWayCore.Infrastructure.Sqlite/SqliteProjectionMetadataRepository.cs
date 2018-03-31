using System;
using EventWayCore.Query;
using SQLite;

namespace EventWayCore.Infrastructure.Sqlite
{
    public class SqliteProjectionMetadataRepository : IProjectionMetadataRepository
    {
        const int CommandTimeout = 600;
        private const string TableName = "ProjectionMetadata";
        private readonly string _connectionString;

        private SQLiteConnection Connect() => new SQLiteConnection(_connectionString);

        public SqliteProjectionMetadataRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void ResetOffsets()
        {
            using (var conn = Connect())
            {

                var sql = $"UPDATE {TableName} SET EventOffset = 0";
                conn.Execute(sql);
            }
        }

        public ProjectionMetadata GetByProjectionId(Guid projectionId)
        {
            using (var conn = Connect())
            {
                var sql = $"SELECT ProjectionId, EventOffset FROM {TableName} WHERE ProjectionId=@projectionId";
                var projection = conn.FindWithQuery<ProjectionMetadataWrapper>(sql, new { projectionId });

                return new ProjectionMetadata(
                    projection.ProjectionId,
                    projection.EventOffset,
                    projection.ProjectionType);
            }
        }

        public void UpdateEventOffset(ProjectionMetadata projectionMetadata)
        {
            using (var conn = Connect())
            {
                const string sql = @"UPDATE {TableName} SET EventOffset = @EventOffset WHERE ProjectionId = @ProjectionId";
                conn.Execute(sql, projectionMetadata);
            }
        }

        public void InitializeProjection(Guid projectionId, string projectionType)
        {
            try
            {
                using (var conn = Connect())
                {
                    const string sql = @"INSERT INTO {TableName} (ProjectionType, ProjectionId, EventOffset) VALUES (@ProjectionType, @ProjectionId, @EventOffset)";

                    var projectionMetadata = new
                    {
                        ProjectionType = projectionType,
                        ProjectionId = projectionId,
                        EventOffset = 0L
                    };

                    conn.Execute(sql, projectionMetadata);
                }
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith("Violation of PRIMARY KEY constraint"))
                    return;

                Console.WriteLine(e);
                throw;
            }
        }
    }
}
