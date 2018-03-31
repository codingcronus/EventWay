using System;
using System.Collections.Concurrent;
using EventWayCore.Query;

namespace EventWayCore.Infrastructure.InMemory
{
    public class InMemoryProjectionMetadataRepository : IProjectionMetadataRepository
    {
        private readonly ConcurrentDictionary<Guid, ProjectionMetadata> _metadataStore = new ConcurrentDictionary<Guid, ProjectionMetadata>();

        public void ResetOffsets()
        {
            foreach (var metadata in _metadataStore.Values)
                metadata.EventOffset = 0L;
        }

        public ProjectionMetadata GetByProjectionId(Guid projectionId)
        {
            return _metadataStore.ContainsKey(projectionId) 
                ? new ProjectionMetadata(projectionId, _metadataStore[projectionId].EventOffset)
                : null;
        }

        public void UpdateEventOffset(ProjectionMetadata projectionMetadata)
        {
            if (_metadataStore.TryGetValue(projectionMetadata.ProjectionId, out var metadata))
                metadata.EventOffset = projectionMetadata.EventOffset;
        }

        public void InitializeProjection(Guid projectionId, string projectionType)
        {
            _metadataStore[projectionId] = new ProjectionMetadata(projectionId, 0L, projectionType);
        }
    }
}