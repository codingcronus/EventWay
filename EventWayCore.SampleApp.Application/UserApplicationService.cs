using System;
using System.Threading.Tasks;
using EventWayCore.Core;
using EventWayCore.Query;
using EventWayCore.SampleApp.Application.Commands;
using EventWayCore.SampleApp.Application.QueryModels;
using EventWayCore.SampleApp.Core;

namespace EventWayCore.SampleApp.Application
{
    public class UserApplicationService
    {
        public UserApplicationService(
            IAggregateStore aggregateStore,
            IQueryModelRepository queryModelRepository)
        {
            if (aggregateStore == null) throw new ArgumentNullException(nameof(aggregateStore));
            if (queryModelRepository == null) throw new ArgumentNullException(nameof(queryModelRepository));

            _aggregateStore = aggregateStore;
            _queryModelRepository = queryModelRepository;
        }

        private readonly IAggregateStore _aggregateStore;
        private readonly IQueryModelRepository _queryModelRepository;

        public async Task<Guid> RegisterUser(RegisterUser command)
        {
            // Check if user already exists
            var existingUser = await _queryModelRepository.QueryItem<UserQueryModel>(x => x.DisplayName == $"{command.FirstName} {command.LastName}");
            if (existingUser != null)
                return existingUser.id;

            // Create aggregate
            var newUserId = Guid.NewGuid();
            var user = new User(newUserId);

            // Create domain command
            var domainCommand = new Core.Commands.RegisterUser(
                command.FirstName,
                command.LastName);

            // Fire command and wait for status response
            user.Tell(domainCommand);

            // Save User aggregate
            // Note: This saves the events published internally by the User Aggregate)
            await _aggregateStore.Save(user);

            return newUserId;
        }
    }
}
