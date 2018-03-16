using System;

namespace EventWay.Query
{
    public class ProjectionMetadata
    {
        public ProjectionMetadata(
            Guid projectionId,
            long eventOffset,
            string projectionType = null)
        {
            ProjectionId = projectionId;
            EventOffset = eventOffset;
            ProjectionType = projectionType;
        }

        public Guid ProjectionId { get; set; }
        public long EventOffset { get; set; }
        public string ProjectionType { get; set; }
    }
}