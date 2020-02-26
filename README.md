# Overview:
This Payment API Repo is an example of a simple WebAPI using .NET Core with Multi-Layered architecture and unit/integration tests. It also demonstrates how to use Dependency Injection using ServiceCollection, Repository Pattern, and MediatR.
# Business Logic:
* Customers can be added, updated, or deleted. They can also top-up their balance
* Staff can be added, updated, or deleted
* Customers can request payments. If they have enough balance, the request will go through, otherwise it will be automatically rejected
* Staff can approve payments. If they approve, the request will be marked as processed. If they reject, the payment will be closed with optional rejection comments
# Assumptions:
* Once deleted, customer or staff data can't be updated. They should also be hidden from the list, although the related payment records should still be shown
* Staff might have different roles other than just approving payments, hence why the property in Payment is called Approver, but the Domain object is called Staff
* Once payments are approved, they will deduct customer's current balance
* Paging is required for payments as they will grow overtime, even for a single customer
* All errors thrown by business logic will be treated as error code 400 (bad request), whereas any other errors will be treated as error code 500 (internal server error)
# Code Architecture
There are multiple layers in this repo:
* **Domain layer**: this layer is where we put business/domain objects as well as busines-related data structures, exceptions, and interfaces. Note that this project should be kept as simple as possible, no references to any third party components and no business logic.
* **Infrastructure layer**: any integration to 3rd party code other than persistence belong to this layer, such as date provider, metrics, etc. Generic infrastructure project should only contain interfaces or system default providers, and specific implementation should be put in specialised projects, e.g. Infrastructure.Metrics.AppInsights or Infrastructure.FeatureFlag.LaunchDarkly. The providers must be consumed through DI.
* **Persistence layer**: any code that deals with persistence/database belong to this layer, such as EF Core, sql server, etc. Generic Persistence project should only contain interfaces, and specific implementation should be put in specialised projects, e.g. Persistence.InMemory, Persistence.SqlServer, etc.  The repositories must be consumed through DI using Repository pattern, regardless whether EF Core is used or not. This will provide flexibility when switching to DB/service that is not supported by EF Core. It will also make unit tests simpler to write.
* **Application layer**: contains complex business logic. Generally implemented using MediatR requests/handlers to minimise the complexity in a single class (one request/function per request/handler) as well as making it reusable in Azure functions, which requires the code to be written in MediatR requests/handlers. Note that MediatR requests/handlers shouldn't call another MediatR requests/handlers, so if the logic inside MediatR handler needs to be reused, it needs to be extracted out into a service.
* **Presentation layer**: the entry points of the code, which can be anything from console app, Web API, Azure functions, etc. The code should mainly focus on setting up and providing entry points, which should rely on either Persistence layer (if only a simple operation is required) or Application layer (for more complex logic) for business logic.
# Test Architecture
There are generally 3 layers of tests:
* **Unit tests**: quick to run and easy to setup. Most of the tests should be unit tests. A unit generally refers to a single class with no/few dependencies, which might connect to a real service such as DB. Unit tests usually cover all logics and edge cases, and should be specific only for the unit they are testing. For example, unit tests for WebAPI should focus on making sure that the URL, http method, content, and params are setup correctly rather than the business logic itself. The tests should validate that the MediatR handlers or the services are called, but they should call the mocked MediatR handlers or services and not the real ones.
* **Integration tests**: slower to run and harder to setup compared to unit tests, but still cheaper compared to end-to-end tests. There shouldn't be a lot of these tests and should only be used to complement what can't be tested by unit tests.
* **End-to-end/UI tests**: slow to run and hard to setup, but will validate the application the same way as user does. As these tests are generally slow to run, the number of the tests should be minimised as much as possible and should only complement what can't be achieved by unit and integration tests, or as a final validation in lieu of manual testing.
# Pre-requisites
* Visual Studio and .NET Core 3.1 SDK installed
* NuGet package manager configured
# Running the API
No configs required, this can be run directly from VS as long as all pre-requisites are met.
# Future Requirements:
* To keep it simple, the API is unversioned at the moment. Ideally, the major version numbers should be pre-determined, but should also be appended by auto-generated minor version number, such as from build number/generated randomly, to prevent version being accidentally overwritten
* Required fields validation in EF Core is only a safeguard. Ideally, the required validation should also be put in front-end. Although it is tempting to make front-end read required fields list from back-end, it is not always the best idea as the fields shown in front-end doesn't necessarily be exactly the same as in DB, and even if they are the same, the fields in front-end screen might come from multiple APIs, which will add complexity when trying to reuse validation schema from back-end.
* If needed, exceptions can contain extra information that can be used by front-end to display more detailed information, which should be returned as JSON in the response
* Infrastructure layer can be added which contains items such as:
  * Logs. This doesn't have to be added as a new project if already using a library that has good abstraction, such as SeriLog
  * Metrics/Telemetry, which is especially useful when investigating production issues (e.g. performance) or when investigating how often a feature is used by customers
  * Authentication, e.g. using MSI/Azure AD (for server to server communication)
  * Audit, e.g. using Audit.NET with table storage
  * Feature flags
  * Any external APIs
* Code coverage can be incorporated in build plan using leaflet, or run manually using VS built-in code coverage report tool
* Dates are stored in UTC to handle multiple timezones. The timezones can be saved per customer or per branch/office (if branch/office concept is introduced). Date formatting is mainly handled by front-end and back-end should just store timezone information and return UTC dates by default.
* Customers might require periodical statements, which might contain balance and list of payments. This can be added easily by adding CustomerStatement domain object, which can be populated by:
  * Azure functions using scheduled messages from Service Bus (https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing#scheduled-messages) if the statement dates are different per customer, or
  * Standard time-triggered Azure functions (https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-scheduled-function) if the statement dates are the same for all customers
* Deployments can be done using ARM template, which can be executed by either cake script or yaml, depending on the build pipeline used (e.g. Bamboo or Azure DevOps), which will also affect how the parameters will be passed