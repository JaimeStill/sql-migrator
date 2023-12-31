﻿using Cli;
using Cli.Commands;

await new CliApp(
    "V2 Data Migrator",
    new()
    {
        new MigrateCommand(),
        new TestCommand()
    }
).InvokeAsync(args);