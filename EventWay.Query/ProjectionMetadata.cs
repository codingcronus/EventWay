using System;

namespace EventWay.Query
{
    public class ProjectionMetadata
    {
        public ProjectionMetadata(
            Guid projectionId,
            long eventOffset)
        {
            ProjectionId = projectionId;
            EventOffset = eventOffset;
        }

        public Guid ProjectionId { get; set; }
        public long EventOffset { get; set; }
    }
}