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