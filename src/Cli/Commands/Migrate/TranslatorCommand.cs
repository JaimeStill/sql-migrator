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

    static async Task Call(string origin, string target, string migrator)
    {
        ConsoleColor color = Console.ForegroundColor;
        string entity = typeof(E).Name;

        try
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Migrating {entity} records...");

            Console.ForegroundColor = ConsoleColor.Gray;
            T? translator = Activator.CreateInstance(typeof(T), new object[] { origin, target, migrator }) as T;

            if (translator is not null)
            {
                List<E> records = await translator.Migrate();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{records.Count} {entity} records successfully migrated!");
            }
        }
        finally
        {
            Console.ForegroundColor = color;
        }
    }
}