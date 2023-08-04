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
                new string[] { "--v1" },
                description: "v1 server and database object in connections.json",
                getDefaultValue: () => "Origin"
            ),
            new Option<string>(
                new string[] { "--v2" },
                description: "v2 server and database object in connections.json",
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
            new ContactInfoCommand(),
            new DepartmentCommand(),
            new EmployeeCommand(),
            new FullCommand()
        }
    )
    { }
}