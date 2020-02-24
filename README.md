# Assumptions:
* Paging is required for payments as they will grow overtime, even for a single customer
# Potential Future Requirements:
* Required fields validation in EF Core is only a safeguard. Ideally, the required validation should also be put in front-end. Although it is tempting to make front-end read required fields list from back-end, it is not always the best idea as the fields shown in front-end doesn't necessarily be exactly the same as in DB, and even if they are the same, the fields in front-end screen might come from multiple APIs, which will add complexity when trying to reuse validation schema from back-end.
* Infrastructure layer can be added which contains items such as:
** Logs. This doesn't have to be added as a new project if already using a library that has good abstraction, such as SeriLog
** Metrics/Telemetry, which is especially useful when investigating production issues (e.g. performance) or when investigating how often a feature is used by customers
** Authentication, e.g. using MSI/Azure AD (for server to server communication)
** Audit, e.g. using Audit.NET with table storage
** Feature flags
** Any external APIs
* Dates are stored in UTC to handle multiple timezones. The timezones can be saved per customer or per branch/office (if branch/office concept is introduced). Date formatting is mainly handled by front-end and back-end should just store timezone information and return UTC dates by default.
* Customers might require periodical statements, which might contain balance and list of payments. This can be added easily by adding CustomerStatement domain object, which can be populated by:
** Azure functions using scheduled messages from Service Bus (https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing#scheduled-messages) if the statement dates are different per customer, or
** Standard time-triggered Azure functions (https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-scheduled-function) if the statement dates are the same for all customers