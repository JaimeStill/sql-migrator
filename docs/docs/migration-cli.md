---
sidebar_position: 4
title: Migration CLI
slug: cli
---

The intended use of the CLI tool is to be able to migrate all or specific models based on migration sub-commands.

For instance, `migrator migrate full` will perform the full data migration while `migrator migrate department` will only migrate `Department` records.

The sections that follow will define the underlying CLI app infrastructure, then walk through building out the CLI app itself.

## Infrastructure

The CLI infrastructure extends the [**System.CommandLine**](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) library to simplify the creation of a CLI app. It is defined in the [`Cli`](https://github.com/JaimeStill/sql-migrator/tree/main/src/Cli) console app project root.

### CliCommand

The [`CliCommand`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/CliCommand.cs) class simplifies the interface for generating a command.

To create an instance of `CliCommand`, you must provide the following:

* `name` - how to call the sub-command (i.e. - `migrate`, `full`, `department`)
* `description` - the description of the command that will be shown when showing help and usage information with `-?`, `-h`, or `--help`.

The following are optional arguments, but at a minimum either `@delegate` or `commands` should be provided:

* `@delegate` - the delegate method to execute when the command is called
* `options` - user-provided CLI arguments whose values are passed to `@delegate`
    * See [`Define options`](https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands?source=recommendations#define-options)
* `commands` - subcommands that are reachable through this command
    * See [`Define subcommands`](https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands?source=recommendations#define-subcommands)

```cs title="CliCommand.cs"
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cli;
public abstract class CliCommand
{
    readonly string name;
    readonly string description;
    readonly Delegate? @delegate;
    readonly List<Option>? options;
    readonly List<CliCommand>? commands;

    public CliCommand(
        string name,
        string description,
        Delegate? @delegate = null,
        List<Option>? options = null,
        List<CliCommand>? commands = null
    )
    {
        this.name = name;
        this.description = description;
        this.@delegate = @delegate;
        this.options = options;
        this.commands = commands;
    }

    public Command Build()
    {
        Command command = new(name, description);

        if (@delegate is not null)
            command.Handler = CommandHandler.Create(@delegate);

        options?.ForEach(command.AddOption);

        if (commands?.Count > 0)
            commands
              .Select(c => c.Build())
              .ToList()
              .ForEach(command.AddCommand);

        return command;
    }
}
```

### CliApp

The [`CliApp`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/CliApp.cs) class simplifies the interface for defining the [root command](https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands?source=recommendations#define-a-root-command).

To create an instance of `CliApp`, you provide the following:

* `description` - the description of the CLI app
* `commands` - the subcommands exposed by the CLI
* `globals` (optional) - the global CLI arguments
    * See [Global options](https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands?source=recommendations#global-options)

```cs title="CliApp.cs"
using System.CommandLine;

namespace Cli;
public class CliApp
{
    readonly RootCommand root;

    public CliApp(
        string description,
        List<CliCommand> commands,
        List<Option>? globals = null
    )
    {
        root = new(description);

        if (globals?.Count > 0)
            globals.ForEach(root.AddGlobalOption);

        commands
            .Select(x => x.Build())
            .ToList()
            .ForEach(root.AddCommand);
    }

    public Task InvokeAsync(params string[] args) =>
        root.InvokeAsync(args);
}
```    

## Building a Migration CLI

The CLI app itself is initialized as a console application via `dotnet new console` at [`Cli`](https://github.com/JaimeStill/sql-migrator/tree/main/src/Cli). Before jumping into building out the migration code, some adjustments to the console app need to be explained.

### Setup

In [`Cli.csproj`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Cli.csproj#L8), the assembly name has been changed to facilitate a CLI-friendly name: `<AssemblyName>migrator</AssemblyName>`. When the project is built and released, this causes the executable to be named `migrator` instead of `Cli`, enabling the full sequence of commands to flow as: `migrator migrate <sub-command>`.

[`Program.cs`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Program.cs) initializes the root command via initializing and invoking a `CliApp`:

:::info
All sub-commands are defined in the [`Commands`](https://github.com/JaimeStill/sql-migrator/tree/main/src/Cli/Commands) sub-directory.
:::

```cs title="Program.cs"
using Cli;
using Cli.Commands;

await new CliApp(
    "V2 Data Migrator",
    new()
    {
        new MigrateCommand(),
        new TestCommand()
    }
).InvokeAsync(args);
```

### MigrateCommand

The `migrate` sub-command is defined as [`MigrateCommand`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Commands/Migrate/MigrateCommand.cs) and is a container for all of the migration sub-commands.

A `migrate` sub-command maps to one or more `Translator` objects. It passes in the provided `--origin`, `--target`, and `--migrator` arguments to the `Translator` constructor and executes the `Translator.Migrate()` method.

It will end up looking like this once all of the commands are built:

```cs title="MigrateCommand.cs"
using System.CommandLine;

namespace Cli.Commands;
public class MigrateCommand : CliCommand
{
    public MigrateCommand() : base(
        "migrate",
        "Test out migration patterns",
        options: new()
        {
            new Option<string>(
                new string[] { "--origin" },
                description: "origin server and database object in connections.json",
                getDefaultValue: () => "Origin"
            ),
            new Option<string>(
                new string[] { "--target" },
                description: "target server and database object in connections.json",
                getDefaultValue: () => "Target"
            ),
            new Option<string>(
                new string[] { "--migrator", "-m" },
                description: "migrator db connection string key in connections.json",
                getDefaultValue: () => "Migration"
            )
        },
        commands: new()
        {
            new CompanyCommand(),
            new ContactInfoCommand(),
            new DepartmentCommand(),
            new EmployeeCommand(),
            new FullCommand()
        }
    )
    { }
}
```

### TranslatorCommand

To simplify the process of defining `migrate` subcommands, an abstract `TranslatorCommand<T, E>` class has been defined that:

* Constrains `T` to `Translator<E>`
* Constrains `E` to `IMigrationTarget`
* Standardizes logging messages to the console
* Initializes a `T` through `Activator.CreateInstance`
* Executes `T.Migrate()`

```cs title="TranslatorCommand.cs"
using Core.Schema;
using Core.Sql;

namespace Cli.Commands;
public abstract class TranslatorCommand<T, E> : CliCommand
where T : Translator<E>
where E : IMigrationTarget
{
    public TranslatorCommand(
        string description
    ) : base(
        typeof(E).Name.ToLower(),
        description,
        new Func<string, string, string, Task>(Call)
    )
    { }

    static async Task Call(string v1, string v2, string migrator)
    {
        ConsoleColor origin = Console.ForegroundColor;
        string entity = typeof(E).Name;

        try
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Migrating {entity} records...");

            Console.ForegroundColor = ConsoleColor.Gray;
            T? translator = Activator.CreateInstance(typeof(T), new object[] { v1, v2, migrator }) as T;

            if (translator is not null)
            {
                List<E> records = await translator.Migrate();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{records.Count} {entity} records successfully migrated!");
            }
        }
        finally
        {
            Console.ForegroundColor = origin;
        }
    }
}
```

The sections that follow will define the migration development workflow, then demonstrate building out all of the infrastructure needed to perform migration of the AdventureWorks schema.

### Migration Workflow

Facilitating data migration requires the following steps:

1. Isolating the target schema from the origin database through a SQL schema query, as demonstrated in the [Schema Design - Identification](/schema-design#identification) documentation.

2. Establishing the initial application infrastructure incorporating the schema, as shown in [Schema Design - Implementation](/schema-design#implementation).

3. Identifying the metadata needed during the migration and deriving a model that implements `IMigrationTarget`, as demonstrated in [Migration Infrastructure - Schema](/infrastructure#schema).

4. Building a [`Translator`](/infrastructure#translator) based on the SQL schema query and target schema.

6. Building a [`CliCommand`](#clicommand) that initializes the `Translator` and executes its `Migrate()` method.

The following sections will demonstrate building out data migration commands with increasing complexity. The sections will consist of:

* A header that indicates what is being migrated
* Links to all of the relevant infrastructure will be provided
* `Translator` and `CliCommand` definitions

:::info
To prevent redundancies and focus on the complexity of a particular `Translator` implementation, concepts will not be repeated between `Translator` explanations.
:::

### Department

File | Description
-----|------------
[department-schema.sql](https://github.com/JaimeStill/sql-migrator/blob/main/queries/department-schema.sql) | SQL Schema Query
[Department.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Schema/Department.cs) | App Schema
[Department.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Schema/AdventureWorks/Department.cs) | `IMigrationTarget`

The [`DepartmentTranslator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Translators/DepartmentTranslator.cs) represents the simplest concrete `Translator` that can be defined.

In the definition, notice that:
* `DepartmentTranslator` derives from `AwTranslator<T>` to simplify the migration workflow.
* The format of `department-schema.sql` is deconstructed to facilitate query generation.
* `override string[] GetProps()` specifies the column selection
* `override string[] RootCommands()` specifies the table to retrieve data from
* `override string[] InsertProps()` specifies the `Department` properties to insert into the target database
* The additional commands provided by `GetByKey` are merged with `RootCommands` through the `AwTranslator.BuildCommands` method

```cs title="DepartmentTranslator.cs"
using Core.Schema.AdventureWorks;

namespace Cli.Translators;
public class DepartmentTranslator : AwTranslator<Department>
{
    public DepartmentTranslator(
        string source = "Origin",
        string target = "Target",
        string migrator = "Migration"
    ) : base(
        source,
        target,
        migrator
    )
    { }

    protected override string[] GetProps() => new string[] {
        "CAST([department].[DepartmentID] as nvarchar(MAX)) [OriginKey],",
        "[department].[Name] [Name],",
        "[department].[GroupName] [Section]"
    };

    protected override string[] RootCommands() => new string[] {
        "FROM [HumanResources].[Department] [department]"
    };

    protected override string[] InsertProps() =>
        InsertProps(new string[] {
            "Name",
            "Section"
        });

    protected override Department ToV1Null() => new()
    {
        OriginKey = "V1Null",
        Name = "V1Null"
    };

    protected override async Task<Department?> GetByKey(string key) =>
        await GetByKey(
            BuildCommands(new string[] {
                $"WHERE [department].[DepartmentID] = '{key}'"
            })
        );

    public override async Task<List<Department>> Get() =>
        await Get(RootCommands());
}
```

```cs title="DepartmentCommand.cs"
using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class DepartmentCommand : TranslatorCommand<DepartmentTranslator, Department>
{
    public DepartmentCommand() : base(
        "Migrate Department from AdventureWorks to V2"
    )
    { }
}
```

Once the command is defined, remember to add it to the `commands` argument of the `MigrateCommand`.

### Employee

File | Description
-----|------------
[employee-schema.sql](https://github.com/JaimeStill/sql-migrator/blob/main/queries/employee-schema.sql) | SQL Schema Query
[Employee.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Schema/Employee.cs) | App Schema
[Employee.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Schema/AdventureWorks/Employee.cs) | `IMigrationTarget`

The [`EmployeeTranslator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Translators/EmployeeTranslator.cs) class represents a more complex implementation of the `AwTranslator` class. Particularly, notice that:

* The SQL queries aggregate data from columns across multiple tables
* `DepartmentTranslator` is initialized to ensure that `Employee.DepartmentId` can be properly initialized.
* The `Translator.OnMigrate` delegate is defined to facilitate setting `Employee.DepartmentId` before the record is inserted.

```cs title="EmployeeTranslator.cs"
using Core.Schema.AdventureWorks;

namespace Cli.Translators;
public class EmployeeTranslator : AwTranslator<Employee>
{
    readonly DepartmentTranslator departmentTranslator;

    public EmployeeTranslator(
        string source = "origin",
        string target = "target",
        string migrator = "migration"
    ) : base(
        source,
        target,
        migrator
    )
    {
        departmentTranslator = new(source, target, migrator);
    }

    protected override Func<Employee, Task<Employee>>? OnMigrate => async (Employee employee) =>
    {
        employee.DepartmentId = await departmentTranslator.EnsureMigrated(employee.OriginDepartmentKey);
        return employee;
    };

    protected override string[] GetProps() => new string[] {
        "CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginKey],",
        "CAST([history].[DepartmentID] as nvarchar(MAX)) [OriginDepartmentKey],",
        "[employee].[NationalIdNumber] [NationalId],",
        "[person].[LastName] [LastName],",
        "[person].[FirstName] [FirstName],",
        "[person].[MiddleName] [MiddleName],",
        "[employee].[LoginId] [Login],",
        "[employee].[JobTitle] [JobTitle]"
    };

    protected override string[] RootCommands() => new string[] {
        "FROM [Person].[person] [person]",
        "LEFT JOIN [HumanResources].[Employee] [employee]",
        "ON [person].[BusinessEntityID] = [employee].[BusinessEntityID]",
        "LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] [history]",
        "ON [employee].[BusinessEntityID] = [history].[BusinessEntityID]",
        "WHERE [person].[PersonType] = 'EM'",
        "AND [history].[EndDate] IS NULL"
    };

    protected override string[] InsertProps() => InsertProps(
        new string[] {
            "DepartmentId",
            "NationalId",
            "LastName",
            "FirstName",
            "MiddleName",
            "Login",
            "JobTitle"
        }
    );

    protected override Employee ToV1Null() => new()
    {
        OriginKey = "V1Null",
        OriginDepartmentKey = "V1Null",
        LastName = "V1Null"
    };

    protected override async Task<Employee?> GetByKey(string key)
    {
        string[] query = BuildCommands(new string[] {
            $"AND [person].[BusinessEntityID] = '{key}'"
        });

        return await GetByKey(
            query
        );
    }

    public override async Task<List<Employee>> Get() =>
        await Get(RootCommands());
}
```

```cs title="EmployeeCommand.cs"
using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class EmployeeCommand : TranslatorCommand<EmployeeTranslator, Employee>
{
    public EmployeeCommand() : base(
        "Migrate Employee from AdventureWorks to V2"
    )
    { }
}
```

Once the command is defined, remember to add it to the `commands` argument of the `MigrateCommand`.

### ContactInfo

File | Description
-----|------------
[contact-info-schema.sql](https://github.com/JaimeStill/sql-migrator/blob/main/queries/contact-info-schema.sql) | SQL Schema Query
[ContactInfo.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Schema/ContactInfo.cs) | App Schema
[ContactInfo.cs](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Schema/AdventureWorks/ContactInfo.cs) | `IMigrationTarget`

Sometimes, translating from the origin schema to the target schema is more complex than simply selecting columns from one or more tables and formatting the selections to conform to the target schema format. In that case, the `AwTranslator` is too restrictive to facilitate proper migration.

The [`ContactInfoTranslator`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Translators/ContactInfoTranslator.cs) demonstrates such a complex migration. Particularly, notice:

* The SQL schema query is actually two select queries whose results are merged through a `UNION`. `GetPhoneQuery()` and `GetEmailQuery()` are used to compose the full data retrieval query in the `Get` and `GetByKey` methods.
* Because the source of `ContactInfo` could be either `Email` or `Phone`, the `OriginKey` property specified by the [`ContactInfo`](https://github.com/JaimeStill/sql-migrator/blob/main/migration/Schemas/AdventureWorks/ContactInfo.cs) `IMigrationTarget` class is actually composed of thre different values: `{OriginEmployeeKey}.{ContactType}.{Value}`. The `GetByKey` method splits out these values to dynamically generate its query.

```cs title="ContactInfoTranslator.cs"
using System.Text;
using Core.Schema.AdventureWorks;
using Core.Sql;

namespace Cli.Translators;
public class ContactInfoTranslator : Translator<ContactInfo>
{
    readonly EmployeeTranslator employeeTranslator;

    public ContactInfoTranslator(
        string source = "origin",
        string target = "target",
        string migrator = "migration"
    ) : base(
        typeof(ContactInfo).Name,
        source,
        target,
        migrator
    )
    {
        employeeTranslator = new(source, target, migrator);
    }

    protected override Func<ContactInfo, Task<ContactInfo>>? OnMigrate => async (info) =>
    {
        info.EmployeeId = await employeeTranslator.EnsureMigrated(info.OriginEmployeeKey);
        return info;
    };

    protected override string[] InsertProps() => new string[] {
        "EmployeeId",
        "Type",
        "ContactType",
        "Value"
    };

    protected override ContactInfo ToV1Null() => new()
    {
        OriginEmployeeKey = "V1Null",
        Value = "V1Null",
        ContactType = "V1Null"
    };

    protected static string[] GetPhoneQuery() => new string[] {
        "SELECT",
        "  CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginEmployeeKey],",
        "  CAST([phone].[PhoneNumber] as nvarchar(MAX)) [Value],",
        "  CAST([phoneType].[Name] as nvarchar(MAX)) [ContactType]",
        "FROM [Person].[Person] [person]",
        "LEFT JOIN [Person].[PersonPhone] [phone]",
        "ON [person].[BusinessEntityID] = [phone].[BusinessEntityID]",
        "LEFT JOIN [Person].[PhoneNumberType] [phoneType]",
        "ON [phone].[PhoneNumberTypeID] = [phoneType].[PhoneNumberTypeID]",
        "WHERE [person].[PersonType] = 'EM'"
    };

    protected static string[] GetEmailQuery() => new string[] {
        "SELECT",
        "  CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginEmployeeKey],",
        "  CAST([email].[EmailAddress] as nvarchar(MAX)) [Value],",
        "  CAST('Email' as nvarchar(MAX)) [ContactType]",
        "FROM [Person].[Person] [person]",
        "LEFT JOIN [Person].[EmailAddress] [email]",
        "ON [person].[BusinessEntityID] = [email].[BusinessEntityID]",
        "WHERE [person].[PersonType] = 'EM'"
    };

    protected override async Task<ContactInfo?> GetByKey(string key)
    {
        string[] split = key.Split('.');
        string employeeId = split[0];
        string contactType = split[1];
        string value = split[2];
        
        StringBuilder query = new();

        switch (contactType)
        {
            case "Email":
                foreach (string line in GetEmailQuery())
                    query.AppendLine(line);

                query.AppendLine($"AND [email].[EmailAddress] = '{value}'");
                break;
            default:
                foreach (string line in GetPhoneQuery())
                    query.AppendLine(line);

                query.AppendLine($"AND [phone].[PhoneNumber] = '{value}'");
                break;
        }

        query.AppendLine($"AND [person].[BusinessEntityID] = `{employeeId}'");

        return await Config
            .Source
            .QueryFirstOrDefault<ContactInfo>(
                query.ToString()
            );
    }

    protected static string GetQuery()
    {
        StringBuilder query = new();

        query.AppendLine("(");
        
        foreach (string line in GetPhoneQuery())
            query.AppendLine($"  {line}");

        query.AppendLine(")");
        query.AppendLine("UNION");
        query.AppendLine("(");

        foreach (string line in GetEmailQuery())
            query.AppendLine($"  {line}");

        query.AppendLine(")");
        query.AppendLine("ORDER BY [Value]");

        return query.ToString();
    }

    public override async Task<List<ContactInfo>> Get()
    {
        string query = GetQuery();

        return await Config
            .Source
            .Query<ContactInfo>(
                query.ToString()
            );
    }
}
```

```cs title="ContactInfoCommand.cs"
using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class ContactInfoCommand : TranslatorCommand<ContactInfoTranslator, ContactInfo>
{
    public ContactInfoCommand() : base(
        "Migrate Contact Info from AdventureWorks to V2"
    )
    { }
}
```

Once the command is defined, remember to add it to the `commands` argument of the `MigrateCommand`.

### Full

To facilitate performing a full migration for each table in the target database, a [`FullCommand`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Commands/Migrate/FullCommand.cs) can be defined that intentionally specifies the order in which to execute the migration:

```cs title="FullCommand.cs"
using Cli.Translators;

namespace Cli.Commands;
public class FullCommand : CliCommand
{
    public FullCommand() : base(
        "full",
        "Migrate V1 data to V2",
        new Func<string, string, string, Task>(Call)
    )
    { }

    static async Task Migrate<T>(string entity, Func<Task<List<T>>> migrate)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Migrating {entity} records...");

        Console.ForegroundColor = ConsoleColor.Gray;
        List<T> data = await migrate();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{data.Count} {entity} records successfully migrated!");
    }

    static async Task Call(string v1, string v2, string migrator)
    {
        ConsoleColor origin = Console.ForegroundColor;

        try
        {
            await Migrate(
                "Department",
                async () => await new DepartmentTranslator(v1, v2, migrator).Migrate()
            );

            await Migrate(
                "Employee",
                async () => await new EmployeeTranslator(v1, v2, migrator).Migrate()
            );

            await Migrate(
                "ContactInfo",
                async () => await new ContactInfoTranslator(v1, v2, migrator).Migrate()
            );
        }
        finally
        {
            Console.ForegroundColor = origin;
        }
    }
}
```