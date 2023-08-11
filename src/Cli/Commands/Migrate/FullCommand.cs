using Cli.Seeders;
using Cli.Translators;

namespace Cli.Commands;
public class FullCommand : CliCommand
{
    public FullCommand() : base(
        "full",
        "Migrate origin schema data to target schema",
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

    static async Task Call(string origin, string target, string migrator)
    {
        ConsoleColor color = Console.ForegroundColor;

        try
        {
            await Migrate(
                "Company",
                async () => await new CompanySeeder(target, migrator).Migrate()
            );

            await Migrate(
                "Department",
                async () => await new DepartmentTranslator(origin, target, migrator).Migrate()
            );

            await Migrate(
                "Employee",
                async () => await new EmployeeTranslator(origin, target, migrator).Migrate()
            );

            await Migrate(
                "ContactInfo",
                async () => await new ContactInfoTranslator(origin, target, migrator).Migrate()
            );
        }
        finally
        {
            Console.ForegroundColor = color;
        }
    }
}