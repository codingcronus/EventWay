# EventWay
EventWay is a modular Event Sourcing + CQRS framework.

Event Sourcing and CQRS (Command Query Responsibility Segregation) works really well together in terms of distributed systems with constant and efficient write-speed + excellent read/query possibilities. Here's a diagram showing the architecture:

## How to setup
1. Create a new Console App (.NET Framework) project
2. Right-click the project and select Manage NuGet Packages
3. Select Browse and search for 'EventWay'.
   1. Install the Async.EventWay.Infrastructure package.
   2. Install the Async.EventWay.Infrastructure.MsSql package.
   3. Install the Async.EventWay.Infrastructure.CosmosDb package.

## Sample Application
This sample application is based on The Onion Architecture (aka Hexagonal Architecture and Ports and Adapters).
If you are not familiar with The Onion Architecture, I highly recommend you to read through Jeffrey Palermos excellent introduction (http://jeffreypalermo.com/blog/the-onion-architecture-part-1).

Introducing this application architecture leads to three projects (or sub-folders in case of the application being very small) in our sample application:
1. **Core**
   -This project contains the domain model and logic. Aggregates, Entity types, Value types and several service interfaces should all be contained here.
2. **Application**
   -This project contains the application services which responsibility is to orchestrate domain logic and hence span entire use cases.
3. **Infrastructure**
   -This project contains the host application (i.e. Web.api webservice, Console App, Xamarin app etc.) and other technology/vendor specific implementations.

We will start in the innermost layer, namely *Core*, and define the sample applications Aggregate class(es).

### Core project
In the sample application we will create an User Aggregate. The user will have a first name and a last name and we should be able to sign up a new user.
To sign up a new user, we'll create a Register User command. There're two kind of commands:
1. Application Commands
2. Domain Commands

Application Commands are sent from the Infrastructure layer to the Application layer. The Domain Commands are sent from the Application layer to an Aggregate in the Domain layer. As a rule of thumb, we always start by defining the Core entities and logic and so we define a Register User **Domain Command**.

#### RegisterUser.cs
```csharp
using EventWay.Core;

namespace EventWay.SampleApp.Core.Commands
{
	public class RegisterUser : IDomainCommand
	{
		public RegisterUser(
			string firstName,
			string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
		}

		public string FirstName { get; private set; }
		public string LastName { get; private set; }
	}
}
```

Once the command is handled by the User Aggregate, one or more events will be published by the aggregate. In our scenario only a single event will be published: The UserRegistered Event. Notice the present tense of commands (an action that is about to be executed) and past tense of events (something that has already occured).

#### UserRegistered.cs
```csharp
using EventWay.Core;

namespace EventWay.SampleApp.Core.Events
{
	public class UserRegistered : DomainEvent
	{
		public UserRegistered(
			string firstName,
			string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
		}

		public string FirstName { get; private set; }
		public string LastName { get; private set; }
	}
}
```
Now that we have defined our Domain Command and Event, we can design the User Aggregate class. This class enforces all the business rules on the User class.

#### User.cs
```csharp
using EventWay.Core;
using EventWay.SampleApp.Core.Commands;
using EventWay.SampleApp.Core.Events;
using System;

namespace EventWay.SampleApp.Core
{
	public class User : Aggregate
	{
		//Internal state
		protected UserState State { get; private set; }

		public User(Guid id) : base(id)
		{
			// Events
			OnEvent<UserRegistered>(e => {
				Console.WriteLine("Got UserRegistered event");

				State = new UserState
				{
					FirstName = e.FirstName,
					LastName = e.LastName
				};
			});

			// Commands
			OnCommand<RegisterUser>(c => {
				Console.WriteLine("Got RegisterUser command");

				if (State != null)
					throw new Exception("User already exists");

				Publish(new UserRegistered(
					c.FirstName,
					c.LastName));
			});
		}

		// The internal state representation
		public class UserState
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
		}
	}
}
```
Notice how the state of the User (first name and last name) can only be changed from within the User Aggregate class. This means that state cannot be accessed from outside the aggregate class and should instead be accessed via Query Models (the *Q* in *CQRS*).

Now that the Core has been fully defined, we can move on to the Application project.

### Application project
TODO: Insert class definitions.

### Infrastructure project
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
