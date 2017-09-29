using System;
using System.Data.SqlClient;
using Dapper;
using EventWay.Query;

namespace EventWay.Infrastructure.MsSql
{
    public class SqlServerProjectionMetadataRepository : IProjectionMetadataRepository
    {
        const int CommandTimeout = 600;

        private readonly string _connectionString;

        public SqlServerProjectionMetadataRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void ResetOffsets()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "UPDATE ProjectionMetadata SET EventOffset = 0";
                conn.Execute(sql, commandTimeout: CommandTimeout);
            }
        }

        public ProjectionMetadata GetByProjectionId(Guid projectionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "SELECT ProjectionId, EventOffset FROM ProjectionMetadata WHERE ProjectionId=@projectionId";
                var projection = conn.QuerySingle<ProjectionMetadata>(sql, new { projectionId }, commandTimeout: CommandTimeout);

                return projection;
            }
        }

        public void UpdateEventOffset(ProjectionMetadata projectionMetadata)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = @"UPDATE ProjectionMetadata SET EventOffset = @EventOffset WHERE ProjectionId = @ProjectionId";
                conn.Execute(sql, projectionMetadata, commandTimeout: CommandTimeout);
            }
        }

        public void InitializeProjection(Guid projectionId, string projectionType)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    const string sql = @"INSERT INTO ProjectionMetadata (ProjectionType, ProjectionId, EventOffset) VALUES (@ProjectionType, @ProjectionId, @EventOffset)";

                    var projectionMetadata = new
                    {
                        ProjectionType = projectionType,
                        ProjectionId = projectionId,
                        EventOffset = 0L
                    };

                    conn.Execute(sql, projectionMetadata, commandTimeout: CommandTimeout);
                }
            }
            catch (SqlException sqlException)
            {
                if (sqlException.Message.StartsWith("Violation of PRIMARY KEY constraint"))
                    return;

                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}