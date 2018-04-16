using System;

namespace EventWayCore.Query
{
    public class ProjectionMetadata
    {
        public ProjectionMetadata(
            Guid projectionId,
            long eventOffset)
            : this(projectionId, eventOffset, null)
        { }

        public ProjectionMetadata(
            Guid projectionId,
            long eventOffset,
            string projectionType)
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