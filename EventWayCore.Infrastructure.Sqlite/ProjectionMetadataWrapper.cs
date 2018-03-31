using System;

namespace EventWayCore.Infrastructure.Sqlite
{
    public class ProjectionMetadataWrapper
    {
        public Guid ProjectionId { get; set; }
        public long EventOffset { get; set; }
        public string ProjectionType { get; set; }
    }
}