using System;

namespace EventWay.Query
{
    public interface IProjectionMetadataRepository
    {
        void ResetOffsets();

        ProjectionMetadata GetByProjectionId(Guid projectionId);
        void UpdateEventOffset(ProjectionMetadata projectionMetadata);
        void InitializeProjection(Guid projectionId, string name);
    }
}