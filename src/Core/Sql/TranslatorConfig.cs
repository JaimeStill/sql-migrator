using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Sql;
public record TranslatorConfig
{
    public Connector Source { get; }
    public Connector Target { get; }
    public MigratorContext Migrator { get; }
    public string Table { get; }
    public string Entity { get; }

    public TranslatorConfig(
        string table,
        string entity,
        string source,
        string target,
        string migrator
    )
    {
        Table = table;
        Entity = entity;
        Source = new(source);
        Target = new(target);

        Migrator = ContextBuilder<MigratorContext>
            .Build(GetConnectionString(migrator));

        Migrator.Database.Migrate();
    }

    static string GetConnectionString(string key)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("connections.json")
            .AddEnvironmentVariables()
            .Build();
            
        return config.GetConnectionString(key)
            ?? throw new Exception($"No connection string was found for {key}");
    }
}