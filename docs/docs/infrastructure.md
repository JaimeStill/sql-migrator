---
sidebar_position: 3
title: Migration Infrastructure
---

With the schema established and translation queries written, I was able to begin working out how to standardize the migration process. The [**Core**](https://github.com/JaimeStill/sql-migrator/tree/main/src/Core) project was built with `dotnet new console` and is setup as a [Hosted Console App](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) to allow Entity Framework migration management. The project defines the building blocks needed for conducting data migrations and consists of the following infrastructure:

* A migration database that keeps track of the data elemenets that have been migrated from the source database.
* A [`Connector`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Connector.cs) class that interfaces with the source and target databases.
* A [`Translator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs) abstract class that standardizes the data migration workflow.

## Migration Database

The [Data](https://github.com/JaimeStill/sql-migrator/tree/main/src/Core/Data) directory contains a [`MigratorContext`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Data/MigratorContext.cs) Entity Framework context defining a single [`MigrationLog`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Data/MigrationLog.cs). The table tracks:

* The origin ID (`OriginId`)
* The target ID (`TargetId`)
* The entity type for the target schema (`TargetType`)

This ensures that before data is inserted into the target database, the migration logs can be queried to determine whether the record and its dependencies have been migrated. Dependencies refer to required relational data needed to define the record being migrated.

## Schema

The [Schema](https://github.com/JaimeStill/sql-migrator/tree/main/src/Core/Schema) directory defines the `IMigrationTarget` interface, which establishes a baseline of metadata needed for conducting data migrations. All entity models that need to be migrated from an origin database need to derive a subclass that implements `IMigrationTarget` for specifying origin schema metadata:

```cs title="IMigrationTarget.cs"
namespace Core.Schema;
public interface IMigrationTarget
{
    public int Id { get; }
    public string Type { get; }
    public string SourceId { get; }
}
```

The [AdventureWorks](https://github.com/JaimeStill/sql-migrator/tree/main/src/Core/Schema/AdventureWorks) directory demonstrates this by providing sub-classes that implement `IMigrationTarget` for all of the [App/Schema](https://github.com/JaimeStill/sql-migrator/tree/main/src/App/Schema) entity models. In addition to the `SourceId`, these models should also specify maps to foreign key IDs in the origin schema. A good example of this is the [`ContactInfo`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Schema/AdventureWorks/ContactInfo.cs) model:

```cs title="ContactInfo.cs"
using AppContactInfo = App.Schema.ContactInfo;

namespace Core.Schema.AdventureWorks;
public class ContactInfo : AppContactInfo, IMigrationTarget
{
    public string SourceId => $"{SourceEmployeeId}.{ContactType}.{Value}";
    public string SourceEmployeeId { get; set; } = string.Empty;
}
```

The `Id` and `Type` properties required by `IMigrationTarget` are already specified by [`App.Schema.ContactInfo`](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Schema/ContactInfo.cs) through its [`Entity`](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Schema/Entity.cs) base class. It simply needs to define the `SourceId` and any metadata needed during the migration process; in this case, `SourceEmployeeId` which maps the origin foreign key ID of the related `Employee` record.

## Connector

The [`Connector`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Connector.cs) class leverages [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) as well as [Dapper](https://github.com/DapperLib/Dapper) to facilitate connecting to a database and performing object-mapped queries. Its constructor is used to build a [`ConnectorConfig`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/ConnectorConfig.cs) that captures the `Server` and `Database` used to initialize the internal `Microsoft.Data.SqlClient.SqlConnection` used to connect to the specified database.

The [`ConnectorCommand`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Commands/Test/ConnectorCommand.cs) test demonstrates working with the `Connector` class:

```cs title="ConnectorCommand.cs"
using Core.Schema.AdventureWorks;
using Core.Sql;

namespace Cli.Commands;
public class ConnectorCommand : CliCommand
{
    public ConnectorCommand() : base(
        "connector",
        "Test out connecting with a Dapper SQL Connector",
        new Func<Task>(Call)
    )
    { }

    static async Task Call()
    {
        string sql = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "department-schema.sql");
        string query = await File.ReadAllTextAsync(sql);

        Connector connector = new("Origin");
        List<Department> departments = await connector.Query<Department>(query);

        departments.ForEach(d => Console.WriteLine($"{d.Name} - {d.Section}"));
    }
}
```

The test command:

1. Reads the embedded [`department-schema.sql`](https://github.com/JaimeStill/sql-migrator/blob/main/queries/department-schema.sql) query into memory.

2. Initializes a new `Connector` instance using a connector key (`new("Origin");`) mapped in [connections.json](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/connections.json#L2). This version of the constructor looks as follows:

    ```cs title="Connector key constructor"
    public Connector(string key)
    {
        IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("connections.json")
          .AddEnvironmentVariables()
          .Build();

        ConnectorConfig result = config
          .GetRequiredSection(key)
          .Get<ConnectorConfig>()
        ?? throw new Exception($"No connector configuration was found for {key}");

        server = result.Server;
        db = result.Database;
    }
    ```

3. The query is executed and mapped to a [`List<Department>`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Schema/AdventureWorks/Department.cs).

4. Each department is written to the console.

<video controls>
    <source src="https://github.com/JaimeStill/sql-migrator/assets/14102723/f726962f-f702-4d44-9130-81f95994356a" />
</video>

## Translator

The [`Translator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs) class is designed to provide a set of baseline functionality to facilitate the [data migration workflow](/cli#migration-workflow). That functionality consists of:

* Initializing a [`TranslatorConfig`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/TranslatorConfig.cs) that provides:
    * Table and entity names relative to generic type `T`
    * Connections to the source, target, and migration databases
* Translating data from an origin schema to a target schema
* Verifying whether it needs to be migrated
* Setting foreign key properties by ensuring that dependecy data has already been migrated
    * If no dependency data is present in the origin database and the relationship is required in the target database, the relationship will be mapped to a `V1Null` record represented on the dependent table.
* Inserting migration data into the target database
* Logging the migration with the migration database

Any class that derives from `Translator` must define:

* [`T ToV1Null()`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L23) - how the data should be formatted to facilitate a missing dependency
* [`string[] InsertProps`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L24) - the properties to include in the `INSERT` query
* [`Task<T?> GetByKey(string key)`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L25) - how to select a single record from the origin database given its `SourceId`
* [`Task<List<T>> Get()`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L26) - how to retrieve all data translated to the target type

The [`Task List<T>> Migrate()`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L28) method performs the migration simply by retreiving the data with `Get()` and passing the results to [`InsertMany(List<T> data)`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L74).

A Translator that represents a model with one or more dependencies on other models can initialize the Translators. It can then ensure the foreign keys for the dependent models are initialized by overriding [`Func<T, Task<T>>? OnMigrate`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L21) and calling the [`Task<int> EnsureMigrated(string key)`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Sql/Translator.cs#L94) method, where `key` is the `SourceId` from the origin database for the dependent data.

### Derived Translators

If there are subsets of models that share similar migration functionality, additional subclasses of `Translator` can be created that define this extended functionality. The [`AwTranslator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Translators/AwTranslator.cs) subclass adds some additional setup for data retrieval and property insertion that simplifies the definition of a majority of *AdventureWorks*-based translators:

```cs title="AwTranslator.cs"
using System.Text;
using Core.Schema;
using Core.Sql;

namespace Cli.Translators;
public abstract class AwTranslator<T> : Translator<T> where T : IMigrationTarget
{
    protected static readonly string[] insertProps = {
        "Type"
    };

    public AwTranslator(
        string source = "Origin",
        string target = "Target",
        string migrator = "Migration"
    ) : base(typeof(T).Name, source, target, migrator)
    { }

    protected abstract string[] RootCommands();
    protected abstract string[] GetProps();

    public string[] InsertProps(string[] props) =>
        insertProps
            .Union(props)
            .ToArray();

    protected string[] BuildCommands(string[] commands) =>
        RootCommands()
            .Concat(commands)
            .ToArray();

    protected string GetQuery(string[] commands, string select)
    {
        StringBuilder query = new();

        query.AppendLine(select);

        foreach (string prop in GetProps())
            query.AppendLine($"  {prop.Trim()}");

        foreach(string command in commands)
            query.AppendLine(command);

        return query.ToString();
    }

    protected async Task<List<T>> Get(string[] commands, string select = "SELECT") =>
        await Config.Source.Query<T>(
            GetQuery(commands, select)
        );

    protected async Task<T?> GetByKey(string[] commands)
    {
        string query = GetQuery(commands, "SELECT TOP(1)");

        return await Config.Source.QueryFirstOrDefault<T>(
            query
        );
    }
}
```

The [Building a Migration CLI](/cli#building-a-migration-cli) section demonstrates building the `Translator` implementations that will facilitate data migration.