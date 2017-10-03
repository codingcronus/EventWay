using System;
using System.Threading;
using System.Threading.Tasks;
using EventWay.Core;
using EventWay.Infrastructure;
using EventWay.Infrastructure.ApplicationInsights;
using EventWay.Infrastructure.CosmosDb;
using EventWay.Infrastructure.MsSql;
using EventWay.SampleApp.Application;
using EventWay.SampleApp.Application.Projections;
using EventWay.SampleApp.Application.QueryModels;
using EventWay.SampleApp.Core.Commands;

namespace EventWay.SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new SampleApp();

            app.Initialize();
            app.Run().Wait();
        }
    }

    class SampleApp
    {
        private UserApplicationService UserApplicationService { get; set; }
        private UserProjection UserProjection { get; set; }

        public async Task Run()
        {
            // Create sample application command
            var registerUser = new Application.Commands.RegisterUser(firstName: "Donald", lastName: "Duck");

            // Invoke sample application service
            var userId = await UserApplicationService.RegisterUser(registerUser);

            // Wait one second for the Read model to be updated.
            // This is much, much more than usually needed.
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
            var offerThroughput = 10000;
            var noOfPartitions = 1000;

            // Event Repository
            var eventRepository = new SqlServerEventRepository(eventDatabaseConnectionString);

            // Projection Metadata Repository
            var projectionMetadataRepository = new SqlServerProjectionMetadataRepository(projectionMetadataDatabaseConnectionString);

            // Query Model Repository
            var queryModelRepository = new DocumentDbQueryModelRepository(cosmosDbDatabaseId, cosmosDbCollectionId,
                offerThroughput, noOfPartitions, cosmosDbEndpoint, cosmosDbAuthKey);
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
