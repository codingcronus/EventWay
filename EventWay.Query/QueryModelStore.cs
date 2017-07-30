using System;
using System.Threading.Tasks;

namespace EventWay.Query
{
    public class QueryModelStore
    {
        public QueryModelStore(
            IQueryModelRepository queryModelRepository,
            IProjectionMetadataRepository projectionMetadataRepository,
            long eventOffset,
            Guid projectionId)
        {
            if (queryModelRepository == null)
                throw new ArgumentNullException(nameof(queryModelRepository));
            if (projectionMetadataRepository == null)
                throw new ArgumentNullException(nameof(projectionMetadataRepository));

            _queryModelRepository = queryModelRepository;
            _projectionMetadataRepository = projectionMetadataRepository;

            _eventOffset = eventOffset;
            _projectionId = projectionId;
        }

        private readonly IQueryModelRepository _queryModelRepository;
        private readonly IProjectionMetadataRepository _projectionMetadataRepository;
        private long _eventOffset;
        private readonly Guid _projectionId;

        public void Initialize()
        {
            
        }

        public async Task<T> GetQueryModel<T>(Guid id, bool createIfMissing = false) where T : QueryModel
        {
            var model = await _queryModelRepository.GetById<T>(id);

            if (model == null && createIfMissing)
                return (T)Activator.CreateInstance(typeof(T), id);

            return model;
        }

        public async Task SaveQueryModel<T>(T queryModel) where T : QueryModel
        {
            //TODO: Wrap in transaction and _eventOffset-- on error

            await _queryModelRepository.Save(queryModel);

            AcknowledgeEvent();
        }

        public void AcknowledgeEvent()
        {
            _eventOffset++;

            var projectionMeta = new ProjectionMetadata(
                _projectionId,
                _eventOffset);

            _projectionMetadataRepository.UpdateEventOffset(projectionMeta);
        }
    }
}
