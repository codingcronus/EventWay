using System;
using System.Linq;
using System.Threading.Tasks;
using EventWay.Core;
using EventWay.Query;
using EventWay.SampleApp.Application.QueryModels;
using EventWay.SampleApp.Core.Events;
using EventWay.SampleApp.Core;

namespace EventWay.SampleApp.Application.Projections
{
    public class UserProjection : Projection
    {
        // Constant ID for projection. Generated from https://www.guidgenerator.com
        private static readonly Guid ProjectionId = Guid.Parse("cb7fdee9-aa3b-4f91-b906-011f4b18e6ec");

        public UserProjection(
            IEventRepository eventRepository,
            IEventListener eventListener,
            IQueryModelRepository queryModelRepository,
            IProjectionMetadataRepository projectionMetadataRepository) : base(ProjectionId, eventRepository, eventListener, queryModelRepository, projectionMetadataRepository)
        {
            projectionMetadataRepository.InitializeProjection(ProjectionId, this.GetType().Name);
        }

        public async Task<UserQueryModel> QueryById(Guid id)
        {
            return await QueryModelRepository.GetById<UserQueryModel>(id);
        }

        public async Task<UserQueryModel[]> QueryAll()
        {
            return (await QueryModelRepository.GetAll<UserQueryModel>()).ToArray();
        }

        public override void Listen()
        {
            // Listen for events
            OnEvent<UserRegistered>(Handle);

            // TODO: Add your events of interest here...

            // Process events for User aggregate
            ProcessEvents<User>().Wait();
        }

        private async Task Handle(UserRegistered @event, Action acknowledgeEvent)
        {
            // Get current instance of query model
            var queryModel = await QueryById(@event.AggregateId) ??
                             QueryModel.CreateQueryModel<UserQueryModel>(@event.AggregateId);

            // Set Query Model properties
            queryModel.FirstName = @event.FirstName;
            queryModel.LastName = @event.LastName;
            queryModel.DisplayName = @event.FirstName + " " + @event.LastName;

            // Create or Update Query model in Read Store (E.g. CosmosDB)
            await QueryModelRepository.Save(queryModel);

            acknowledgeEvent();
        }
    }
}
