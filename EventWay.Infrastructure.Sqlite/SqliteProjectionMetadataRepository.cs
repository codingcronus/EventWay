using System;
using System.Data.SQLite;
using Dapper;
using EventWay.Query;

namespace EventWay.Infrastructure.Sqlite
{
    public class SqliteProjectionMetadataRepository : IProjectionMetadataRepository
    {
        const int CommandTimeout = 600;
        private const string TableName = "ProjectionMetadata";
        private readonly string _connectionString;

        private SQLiteConnection Connect() => new SQLiteConnection(_connectionString).AsOpen();

        public SqliteProjectionMetadataRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void ResetOffsets()
        {
            using (var conn = Connect())
            {
                conn.Open();

                var sql = $"UPDATE {TableName} SET EventOffset = 0";
                conn.Execute(sql, commandTimeout: CommandTimeout);
            }
        }

        public ProjectionMetadata GetByProjectionId(Guid projectionId)
        {
            using (var conn = Connect())
            {
                conn.Open();

                var sql = $"SELECT ProjectionId, EventOffset FROM {TableName} WHERE ProjectionId=@projectionId";
                var projection = conn.QuerySingle<ProjectionMetadata>(sql, new { projectionId }, commandTimeout: CommandTimeout);

                return projection;
            }
        }

        public void UpdateEventOffset(ProjectionMetadata projectionMetadata)
        {
            using (var conn = Connect())
            {
                conn.Open();

                const string sql = @"UPDATE {TableName} SET EventOffset = @EventOffset WHERE ProjectionId = @ProjectionId";
                conn.Execute(sql, projectionMetadata, commandTimeout: CommandTimeout);
            }
        }

        public void InitializeProjection(Guid projectionId, string projectionType)
        {
            try
            {
                using (var conn = Connect())
                {
                    conn.Open();

                    const string sql = @"INSERT INTO {TableName} (ProjectionType, ProjectionId, EventOffset) VALUES (@ProjectionType, @ProjectionId, @EventOffset)";

                    var projectionMetadata = new
                    {
                        ProjectionType = projectionType,
                        ProjectionId = projectionId,
                        EventOffset = 0L
                    };

                    conn.Execute(sql, projectionMetadata, commandTimeout: CommandTimeout);
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
