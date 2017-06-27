# EventWay
EventWay is a modular Event Sourcing + CQRS framework.

## How to setup
1. Create a new Console App (.NET Framework) project
2. Right-click the project and select Manage NuGet Packages
3. Select Browse and search for 'EventWay'.
  1. Install the Async.EventWay.Infrastructure package.
  2. Install the Async.EventWay.Infrastructure.MsSql package.
  3. Install the Async.EventWay.Infrastructure.CosmosDb package.

## Sample Application
1. Update the configuration parameters in the Initialize method
2. Invoke the Initialize method
3. Invoke the Run method

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

using EventWay.Core;

using EventWay.Infrastructure;
using EventWay.Infrastructure.CosmosDb;
using EventWay.Infrastructure.MsSql;

using EventWay.SampleApp.Application.QueryModels;
using EventWay.SampleApp.Application.Projections;
using EventWay.SampleApp.Application;
using EventWay.SampleApp.Application.Commands;

namespace EventWay.SampleApp
{
	public class SampleApp
	{
		private UserApplicationService UserApplicationService { get; set; }
		private UserProjection UserProjection { get; set; }

		public async Task Run()
		{
			// Initialize EventWay framework
			Initialize();

			// Create sample application command
			var registerUser = new RegisterUser(firstName: "Donald", lastName: "Duck");

			// Invoke sample application service
			var userId = await UserApplicationService.RegisterUser(registerUser);

			// Wait one second for the Read model to be updated.
			// This is much more than usually needed.
			Thread.Sleep(1000);

			// Get the query model
			UserQueryModel queryModel = await UserProjection.QueryById(userId);

			Console.WriteLine($"Query models Display Name: {queryModel.DisplayName}");

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		public void Initialize()
		{
			// Configuration Parameters
			var eventDatabaseConnectionString = "Data Source=localhost;Initial Catalog=eventway-sample-db;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
			var projectionMetadataDatabaseConnectionString = eventDatabaseConnectionString;

			var cosmosDbEndpoint = "https://localhost:8081"; // This is the default endpoint for local emulator-instances of the Cosmos DB
			var cosmosDbAuthKey = "<REPLACE WITH YOUR COSMOS DB AUTH KEY>";
			var cosmosDbDatabaseId = "eventway-sample-db";
			var cosmosDbCollectionId = "projections";

			// Event Repository
			var eventRepository = new SqlServerEventRepository(eventDatabaseConnectionString, createEventsTable: true);

			// Projection Metadata Repository
			var projectionMetadataRepository = new SqlServerProjectionMetadataRepository(projectionMetadataDatabaseConnectionString, createProjectionMetadataTable: true);

			// Query Model Repository
			var queryModelRepository = new DocumentDbQueryModelRepository(cosmosDbDatabaseId, cosmosDbCollectionId, cosmosDbEndpoint, cosmosDbAuthKey);
			queryModelRepository.Initialize();

			// Event Listener
			var eventListener = new BasicEventListener();

			// Aggregate services
			var aggregateFactory = new DefaultAggregateFactory();
			var aggregateRepository = new AggregateRepository(eventRepository, aggregateFactory);
			var aggregateStore = new AggregateStore(aggregateRepository, eventListener);

			// PROJECTIONS
			UserProjection = new UserProjection(
				eventRepository,
				eventListener,
				queryModelRepository,
				projectionMetadataRepository);

			// APPLICATION SERVICES
			UserApplicationService = new UserApplicationService(
				aggregateStore,
				queryModelRepository);

			// Start listening for events
			UserProjection.Listen();
		}
	}
}
    ```
