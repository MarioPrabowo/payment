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
# Potential Future Requirements:
* Required fields validation in EF Core is only a safeguard. Ideally, the required validation should also be put in front-end. Although it is tempting to make front-end read required fields list from back-end, it is not always the best idea as the fields shown in front-end doesn't necessarily be exactly the same as in DB, and even if they are the same, the fields in front-end screen might come from multiple APIs, which will add complexity when trying to reuse validation schema from back-end.
* If needed, exceptions can contain extra information that can be used by front-end to display more detailed information, which should be returned as JSON in the response
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
* Deployments can be done using ARM template, which can be executed by either cake script or yaml, depending on the build pipeline used (e.g. Bamboo or Azure DevOps), which will also affect how the parameters will be passed