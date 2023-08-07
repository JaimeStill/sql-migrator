---
sidebar_position: 2
title: Schema Design
---

:::note
All of the queries specified below can be found in the [queries](https://github.com/JaimeStill/sql-migrator/tree/main/queries/) directory.
:::

To help isolate a simple schema to use for proving out the goals of this repository, I built the following query to explore extended properties associated with a `[Person].[Person]` object:

```sql title="aw-person-relationships.sql"
SELECT
*
FROM [Person].[Person] as person
LEFT JOIN [Person].[BusinessEntityContact] [contact]
ON [person].BusinessEntityID = [contact].PersonID
LEFT JOIN [Person].[EmailAddress] [email]
ON [person].BusinessEntityID = [email].BusinessEntityID
LEFT JOIN [Person].[Password] [password]
ON [person].BusinessEntityID = [password].BusinessEntityID
LEFT JOIN [Person].[PersonPhone] [phone]
ON [person].BusinessEntityID = [phone].BusinessEntityID
LEFT JOIN [HumanResources].[Employee] [employee]
ON [person].BusinessEntityID = [employee].BusinessEntityID
LEFT JOIN [Sales].[Customer] [customer]
ON [person].BusinessEntityID = [customer].PersonID
LEFT JOIN [Sales].[PersonCreditCard] [card]
ON [person].BusinessEntityID = [card].BusinessEntityID
```

From this, I determined that I wanted to capture simple personnel data for employees and their contact info. `Employee` would have `ContactInfo` and `ContactInfo` would aggregate both the `[Person].[EmailAddress]` and `[Person].[PersonPhone]` tables.

I built the following query to explore extended properties associated with a `[HumanResources].[Employee]` object:

```sql title="aw-employee-relationships.sql"
SELECT
*
FROM [HumanResources].[Employee] [employee]
LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] [deptHistory]
ON [employee].[BusinessEntityID] = [deptHistory].[BusinessEntityID]
LEFT JOIN [HumanResources].[Department] [department]
ON [deptHistory].[DepartmentID] = [department].[DepartmentID]
LEFT JOIN [HumanResources].[EmployeePayHistory] [pay]
ON [employee].[BusinessEntityID] = [pay].[BusinessEntityID]
LEFT JOIN [HumanResources].[JobCandidate] [candidate]
ON [employee].[BusinessEntityID] = [candidate].[BusinessEntityID]
LEFT JOIN [Person].[Person] [person]
ON [employee].[BusinessEntityID] = [person].[BusinessEntityID]
LEFT JOIN [Sales].[SalesPerson] [sales]
ON [employee].[BusinessEntityID] = [sales].[BusinessEntityID]
LEFT JOIN [Purchasing].[PurchaseOrderHeader] [po]
ON [employee].[BusinessEntityID] = [po].[EmployeeID]
ORDER BY [employee].[LoginID]
```

From this, I determined that I wanted to capture the current `[HumanResources].[Department]` the `Employee` currently belongs to as well.

## Schema Design

In order to isolate the final properties for my schema models, I built queries that translated the AdventureWorks data into the format that I wanted. In addition to these properties, I also specified `Source*` properties that would be used to facilitate data migration from AdventureWorks into my own database.

### Department Schema

```sql title="department-schema.sql"
SELECT DISTINCT
    CAST([department].[DepartmentID] as nvarchar(MAX)) [SourceId],
    [department].[Name] [Name],
    [department].[GroupName] [GroupName]
FROM [HumanResources].[Department] [department]
ORDER BY [department].[Name]
```

### Employee Schema

```sql title="employee-schema.sql"
SELECT
    CAST([person].[BusinessEntityID] as nvarchar(MAX)) [SourceId],
    CAST([history].[DepartmentID] as nvarchar(MAX)) [SourceDepartmentId],
    [employee].[NationalIdNumber] [NationalId],
    [person].[LastName] [LastName],
    [person].[FirstName] [FirstName],
    [person].[MiddleName] [MiddleName],
    [employee].[LoginId] [Login],
    [employee].[JobTitle] [JobTitle]
FROM [Person].[Person] [person]
LEFT JOIN [HumanResources].[Employee] [employee]
ON [person].[BusinessEntityID] = [employee].[BusinessEntityID]
LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] [history]
ON [employee].[BusinessEntityID] = [history].[BusinessEntityID]
WHERE [person].[PersonType] = 'EM'
AND [history].[EndDate] IS NULL
ORDER BY [person].[LastName]
```

In the above query, I wanted to filter the results so that I only get:

* People who are Employees: `WHERE [person].[PersonType] = 'EM'`
* EmployeeDepartmentHistory only for the **current** Department: `AND [history].[EndDate] IS NULL`

### Contact Info Schema

```sql title="contact-info-schema.sql"
(
    SELECT
        CAST([person].[BusinessEntityID] as nvarchar(MAX)) [SourceEmployeeId],
        CAST([phone].[PhoneNumber] as nvarchar(MAX)) [Value],
        CAST([phoneType].[Name] as nvarchar(MAX)) [ContactType]
    FROM [Person].[Person] [person]
    LEFT JOIN [Person].[PersonPhone] [phone]
    ON [person].[BusinessEntityID] = [phone].[BusinessEntityID]
    LEFT JOIN [Person].[PhoneNumberType] [phoneType]
    ON [phone].[PhoneNumberTypeID] = [phoneType].[PhoneNumberTypeID]
)
UNION
(
    SELECT
        CAST([person].[BusinessEntityID] as nvarchar(MAX)) [SourceEmployeeId],
        CAST([email].[EmailAddress] as nvarchar(MAX)) [Value],
        CAST('Email' as nvarchar(MAX)) [ContactType]
    FROM [Person].[Person] [person]
    LEFT JOIN [Person].[EmailAddress] [email]
    ON [person].[BusinessEntityID] = [email].[BusinessEntityID]
)
ORDER BY [Value]
```

The above query merges the data from `[Person.PersonPhone]`, `[Person].[PhoneNumberType]`, and `[Person].[EmailAddress]` into a common schema that can be used for `ContactInfo`.

## Implementation

To prepare the project for building out the data migration infrastructure, I initialized the following projects:

### [App](https://github.com/JaimeStill/sql-migrator/tree/main/src/App)

Built with `dotnet new webapi` to serve as an API for the app schema and contains the Entity Framework configuration. It has the following structure:

* [Schema](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Schema) - class definitions for Entity Framework models.
* [Data](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Data) - contains the Entity Framework `AppDbContext`.
    * [Config](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Data/Config) - contains [`IEntityTypeConfiguration`](https://learn.microsoft.com/en-us/ef/core/modeling/#grouping-configuration) entity configurations.
* [Migrations](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Migrations) - contains the Entity Framework schema migrations.
* [Services](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Services) - contains classes that define entity service logic and a class for registering the services with the [ASP.NET Core Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) service container.
* [Controllers](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Controllers) - contains API controllers that expose entity logic.

### [Core](https://github.com/JaimeStill/sql-migrator/tree/main/src/Core)

Built with `dotnet new console` to facilitate defining the core migration infrastructure and migration database schema.  It is setup as a [Hosted Console App](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) to facilitate Entity Framework migration management. It has the following structure:

### [Cli](https://github.com/JaimeStill/sql-migrator/tree/main/src/Cli)



It has the following structure:


