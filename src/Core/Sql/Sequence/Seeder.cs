using System.Text;
using Core.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Sql;
public abstract class Seeder<E> : Sequence<E, SequenceConfig<E>>
where E : IMigrationTarget
{
    public override SequenceConfig<E> Config { get; set; }

    public Seeder(
        string target = "Target",
        string migrator = "Migration"
    )
    {
        Config = new(target, migrator);
    }

    public E Default => Records.First();
    public abstract List<E> Records { get; }

    protected abstract string[] FindCommands(E record);

    public override async Task<List<E>> Migrate() =>
        await InsertMany(Records);

    public async Task<int> EnsureMigrated(E record)
    {
        int? result = await FindId(record);

        if (result is null)
        {
            record = (await Insert(record)).Data;

            return record.Id;
        }
        else
            return result.Value;
    }

    protected override async Task<InsertResult<E>> Insert(E record)
    {
        if (OnInsert is not null)
            record = await OnInsert(record);

        int? id = await Verify(record);

        if (id is null)
        {
            if (OnMigrate is not null)
                record = await OnMigrate(record);

            record.Id = (await Config.Target.Insert(InsertQuery(), record)).Id;

            Console.WriteLine($"Logging {record.Type} seed: {record.Id}");
            await Config.MigrateLog(record.OriginKey, record.Id, record.Type);

            if (AfterMigrate is not null)
                await AfterMigrate(record);

            return new(record, true);
        }
        else
        {
            record.Id = id.Value;
            return new(record);
        }
    }

    protected override async Task<int?> Verify(E record)
    {
        int? check = await FindId(record);

        if (check is not null)
        {
            bool migrated = await Config
                .Migrator
                .MigrationLogs
                .AnyAsync(x =>
                    x.OriginId == record.OriginKey
                    && x.TargetId == check.Value
                    && x.TargetType == record.Type
                );

            if (!migrated)
                await Config.MigrateLog(record.OriginKey, check.Value, record.Type);
                
            return check;
        }
        else
            return check;
    }

    protected string FindIdQuery(E record)
    {
        StringBuilder query = new();

        query.AppendLine("SELECT TOP(1)");
        query.AppendLine("  [e].[Id]");
        query.AppendLine($"FROM [{Config.Entity}] [e]");

        foreach (string command in FindCommands(record))
            query.AppendLine(command);

        return query.ToString();
    }

    protected async Task<int?> FindId(E record)
    {
        int? result = await Config.Target.QueryFirstOrDefault<int?>(
            FindIdQuery(record)
        );

        return result;
    }
}