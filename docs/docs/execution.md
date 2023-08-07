---
sidebar_position: 5
title: Execute Data Migration
---

Before performing the migration, you need to be sure that your environment is properly prepared. At the time of writing this documentation, my environment consists of the following:

Tool | Version
-----|--------
[.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) | 7.0.306
[`dotnet-ef`](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) | 7.0.9
[SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) | 2022 Developer Edition
[SQL Server Management Studio](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16) | 19.1
[Azure Data Studio](https://learn.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio?view=sql-server-ver16&tabs=redhat-install%2Credhat-uninstall) | 1.45.0


Additionally, you should have access to a SQL Server with [AdventureWorks](https://learn.microsoft.com/en-us/sql/samples/adventureworks-install-configure?view=sql-server-ver16&tabs=ssms) installed.

The database configuration can be found in [connections.json](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/connections.json):

```json title="connections.json"
{
    "Origin": {
        "Server": ".\\DevSql",
        "Database": "AdventureWorks2022"
    },
    "Target": {
        "Server": ".\\DevSql",
        "Database": "v2-schema"
    },
    "ConnectionStrings": {
        "Migration": "Server=.\\DevSql;Trusted_Connection=true;TrustServerCertificate=true;Database=v2-migration"        
    }
}
```

:::tip
If you need to consolidate multiple origin databases into a single target database, run the migration with different `connections.json` files where each **origin** database has a corresponding  **origin** `ConnectorConfig` and **migration** connection string.
:::

The migration and target databases should always be in sync. If on of these databases is removed and you want to perform a new migration, the other database should also be removed prior to the migration.

## Database Preparation

The migration and target databases have corresponding Entity Framework configuration in this project:

* Migration - [MigratorContext](https://github.com/JaimeStill/sql-migrator/blob/main/src/Core/Data/MigratorContext.cs)
* Target - [AppDbContext](https://github.com/JaimeStill/sql-migrator/blob/main/src/App/Data/AppDbContext.cs)

This project also defines tasks for dropping and updating these databases:

* [`app-db-drop`](https://github.com/JaimeStill/sql-migrator/blob/main/.vscode/tasks.json#L5)
* [`app-db-update`](https://github.com/JaimeStill/sql-migrator/blob/main/.vscode/tasks.json#L19)
* [`migration-db-drop`](https://github.com/JaimeStill/sql-migrator/blob/main/.vscode/tasks.json#L40)
* [`migration-db-update`](https://github.com/JaimeStill/sql-migrator/blob/main/.vscode/tasks.json#L54)

Alternatively, these commands can be executed from a terminal:

Task | Directory | Command
-----|-----------|--------
Drop App DB | */src/App* | `dotnet ef database drop -f`
Update App DB | */src/App* | `dotnet ef database update`
Drop Migration DB | */src/Core* | `dotnet ef database drop -f`
Update Migration DB | */src/Core* | `dotnet ef database update`

:::info
Each of the following section start off assuming that you are starting with a blank migration and target database. The CLI automatically initializes the migration database according to the connection string if it does not exist, so you only need to drop the database between CLI runs. The target database, however, does not and needs to be dropped and updated between CLI runs.

Additionally, all of the CLI runs are executed from the `/src/Cli/bin/Debug/net7.0/` directory after the project has been built.
:::

## Migrate a Single Entity

The simplest form of data migration is to migrate a table that has no dependencies via its migration command. Doing so will only import the data from that specific table and nothing else.

To demonstrate this, run `.\migrator.exe migrate department`:

<video controls>
    <source src="https://github.com/JaimeStill/sql-migrator/assets/14102723/a2b59173-53fb-443c-bfd2-51583ea776e0" />
</video>

Querying the target database, you can see that the `Department` records have been successfully migrated:

![Migrate Department](/img/light/migrate-department.png#gh-light-mode-only)![Migrate Department](/img/dark/migrate-department.png#gh-dark-mode-only)

Additionally, querying the migration database, you can see that the recorded migration logs:

![Migrate Department Logs](/img/light/migrate-department-logs.png#gh-light-mode-only)![Migrate Department Logs](/img/dark/migrate-department-logs.png#gh-dark-mode-only)

## Migrate Entities Recursively

If you're performing a fresh migrating targeting a table with dependencies, the migrated data will include the records for the dependent data along with the targeted table records.

To demonstrate this, run `.\migrator.exe migrate employee`:

<video controls>
    <source src="https://github.com/JaimeStill/sql-migrator/assets/14102723/86c6b462-fb0b-4645-a5ef-5ac0561e4ba9" />
</video>

Querying the target database, you can see that the `Employee` records have been migrated along with their dependent `Department` records:

![Migrate Recursive](/img/light/migrate-recursive.png#gh-light-mode-only)![Migrate Recursive](/img/dark/migrate-recursive.png#gh-dark-mode-only)

:::info
If `.\migrator.exe migrate contactinfo` had been run instead, all `ContactInfo` records would have also caused their dependent `Employee` records to be migrated as well as their dependent `Department` records.

By building in calls to `Translator.EnsureMigrated` for each dependency inside of an `OnMigrate` callback, we can ensure that the full dependency tree is migrated recursively.
:::

Additionally, querying the migration database, you can see the recorded migration logs:

![Migrate Recursive Logs](/img/light/migrate-recursive-logs.png#gh-light-mode-only)![Migrate Recursive](/img/dark/migrate-recursive-logs.png#gh-dark-mode-only)

## Full Migration

Performing a full migration will execute the `Translator.Migrate()` migrations in the order they are specified in the [`FullCommand`](https://github.com/JaimeStill/sql-migrator/blob/main/src/Cli/Commands/Migrate/FullCommand.cs).

To demonstrate this, run `.\migrator.exe migrate full`:

<video controls>
    <source src="https://github.com/JaimeStill/sql-migrator/assets/14102723/cd75c6bc-885f-47c8-9b6e-3d83c027e8cd" />
</video>

Querying the target database, you can see that all records from all tables have been migrated:

![Migrate Full](/img/light/migrate-full.png#gh-light-mode-only)![Migrate Full](/img/dark/migrate-full.png#gh-dark-mode-only)

:::info
If data were ever migrated as a dependency before the migration for its table is executed, that record would be skipped during the validation phase since the record with that key would already be in the migration database. This ensures that there is no duplication of records polluting the migration.
:::

Additionally, querying the migration database, you can see the recorded migration logs:

![Migrate Full Logs](/img/light/migrate-full-logs.png#gh-light-mode-only)![Migrate Full Logs](/img/dark/migrate-full-logs.png#gh-dark-mode-only)