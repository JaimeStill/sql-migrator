using Core.Schema;
using Core.Sql;

namespace Cli.Commands;
public abstract class SeederCommand<S, E> : CliCommand
where S : Seeder<E>
where E : IMigrationTarget
{
    public SeederCommand(
        string description
    ) : base(
        typeof(E).Name.ToLower(),
        description,
        new Func<string, string, string, Task>(Call)
    )
    { }

    public static async Task Call(string origin, string target, string migrator)
    {
        ConsoleColor color = Console.ForegroundColor;
        string entity = typeof(E).Name;

        try
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Seeding {entity} records...");

            Console.ForegroundColor = ConsoleColor.Gray;
            S? seeder = Activator.CreateInstance(typeof(S), new object[] { target, migrator }) as S;

            if (seeder is not null)
            {
                List<E> records = await seeder.Migrate();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{records.Count} {entity} records successfully seeded!");
            }
        }
        finally
        {
            Console.ForegroundColor = color;
        }
    }
}