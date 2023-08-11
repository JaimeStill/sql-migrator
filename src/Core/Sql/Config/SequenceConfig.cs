using Core.Data;
using Core.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Sql;
public record SequenceConfig<E>
where E : IMigrationTarget
{
    public const string NoOriginKey = "N/A";
    public string Entity => typeof(E).Name;
    public string TargetKey { get; }
    public string MigratorKey { get; }
    public Connector Target { get; }
    public MigratorContext Migrator { get; }

    public SequenceConfig(
        string target,
        string migrator
    )
    {
        TargetKey = target;
        MigratorKey = migrator;

        Target = new(target);

        Migrator = ContextBuilder<MigratorContext>
            .Build(GetConnectionString(migrator));

        Migrator.Database.Migrate();
    }

    public async Task MigrateLog(string originId, int targetId, string targetType)
    {
        await Migrator.MigrationLogs.AddAsync(new MigrationLog
        {
            OriginId = originId,
            TargetId = targetId,
            TargetType = targetType
        });

        await Migrator.SaveChangesAsync();
    }

    public async Task ClearLog(string originId, int targetId, string targetType)
    {
        MigrationLog? log = await Migrator
            .MigrationLogs
            .FirstOrDefaultAsync(x =>
                x.OriginId == originId
                && x.TargetId == targetId
                && x.TargetType == targetType
            );

        if (log is not null)
        {
            Migrator.MigrationLogs.Remove(log);
            await Migrator.SaveChangesAsync();
        }
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