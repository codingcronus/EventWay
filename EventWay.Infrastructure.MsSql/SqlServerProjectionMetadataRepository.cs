using System;
using System.Data.SqlClient;
using Dapper;
using EventWay.Query;

namespace EventWay.Infrastructure.MsSql
{
    public class SqlServerProjectionMetadataRepository : IProjectionMetadataRepository
    {
        private readonly string _connectionString;

        public SqlServerProjectionMetadataRepository(string connectionString, bool createProjectionMetadataTable = false)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;

            if (createProjectionMetadataTable)
            {
                //TODO: Create Projection Metadata Table
                /*
                 CREATE TABLE ProjectionMetadata(
                        ProjectionType nvarchar(200) not null,
                        ProjectionId uniqueidentifier not null,
		                EventOffset bigint not null,
                        Constraint PKProjectionMetadata PRIMARY KEY(ProjectionId)
                    )

                CREATE INDEX Idx_ProjectionMetadata_ProjectionId
                ON ProjectionMetadata(ProjectionId)
                GO
                 */
            }
        }

        public void ResetOffsets()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "UPDATE ProjectionMetadata SET EventOffset = 0";
                conn.Execute(sql);
            }
        }

        public ProjectionMetadata GetByProjectionId(Guid projectionId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = "SELECT ProjectionId, EventOffset FROM ProjectionMetadata WHERE ProjectionId=@projectionId";
                var projection = conn.QuerySingle<ProjectionMetadata>(sql, new { projectionId });

                return projection;
            }
        }

        public void UpdateEventOffset(ProjectionMetadata projectionMetadata)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = @"UPDATE ProjectionMetadata SET EventOffset = @EventOffset WHERE ProjectionId = @ProjectionId";
                conn.Execute(sql, projectionMetadata);
            }
        }

        public void InitializeProjection(Guid projectionId, string projectionType)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    const string sql =
                        @"INSERT INTO ProjectionMetadata (ProjectionType, ProjectionId, EventOffset) VALUES (@ProjectionType, @ProjectionId, @EventOffset)";

                    var projectionMetadata = new
                    {
                        ProjectionType = projectionType,
                        ProjectionId = projectionId,
                        EventOffset = 0L
                    };

                    conn.Execute(sql, projectionMetadata);
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

        public void ClearProjections()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                const string sql = @"UPDATE ProjectionMetadata SET EventOffset = 0";
                conn.Execute(sql);
            }
        }
    }
}