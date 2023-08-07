using System.CommandLine;
using System.Data.Common;
using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Cli.Commands;
public class ContextBuilderCommand : CliCommand
{
    public ContextBuilderCommand() : base(
        "contextbuilder",
        "Test MigratorContext initialization with ContextBuilder",
        new Func<string, Task>(Call),
        new()
        {
            new Option<string>(
                new string[] { "--key", "-k" },
                description: "Connection string key in connections.json",
                getDefaultValue: () => "Migration"
            )
        }
    )
    { }

    static async Task Call(string key)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("connections.json")
            .AddEnvironmentVariables()
            .Build();

        string connection = config.GetConnectionString(key)
            ?? throw new Exception($"No connection string was found for {key}");

        Console.WriteLine("Building MigratorContext");
        using MigratorContext ctx = ContextBuilder<MigratorContext>.Build(connection);
        Console.WriteLine($"Connection: {ctx.Database.GetConnectionString()}");
        Console.WriteLine();

        DbConnection db = ctx.Database.GetDbConnection();

        Console.WriteLine("Opening Database Connection");
        await ctx.Database.OpenConnectionAsync();
        Console.WriteLine($"Connection State: {db.State}");
        Console.WriteLine();

        Console.WriteLine("Closing Database Connection");
        ctx.Database.CloseConnection();
        Console.WriteLine($"Connection State: {db.State}");
    }
}