using Core.Data;
using Core.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Sql;
public abstract class Translator<E> : Sequence<E, TranslatorConfig<E>>
where E : IMigrationTarget
{
    protected const string V1Null = "V1Null";
    public override TranslatorConfig<E> Config { get; set; }

    public Translator(
      string source = "Origin",
      string target = "Target",
      string migrator = "Migration"
    )
    {
        Config = new(source, target, migrator);
    }

    protected abstract E ToV1Null();
    protected abstract Task<E?> GetByKey(string key);
    public abstract Task<List<E>> Get();

    public override async Task<List<E>> Migrate()
    {
        List<E> data = await Get();
        return await InsertMany(data);
    }

    /*
      Given the origin key for a record, ensure that the
      corresponding record has been migrated.
      Returns the target key for the migrated record.
    */
    public async Task<int> EnsureMigrated(string key)
    {
        int id = await GetTargetIdFromOriginKey(key);

        if (id < 1)
        {
            E entity = (key == V1Null
                ? ToV1Null()
                : await GetByKey(key))
            ?? throw new Exception($"Provided key {key} references a non-existent {Config.Entity}");

            entity = (await Insert(entity)).Data;

            id = entity.Id;
        }

        return id;
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

            Console.WriteLine($"Migrating {record.Type} migration: {record.OriginKey} => {record.Id}");
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

    protected async Task<int> GetTargetIdFromOriginKey(string key) =>
        await Config
            .Migrator
            .MigrationLogs
            .Where(x =>
                x.OriginId == key
                && x.TargetType == typeof(E).FullName
            )
            .Select(x => x.TargetId)
            .FirstOrDefaultAsync();

    protected override async Task<int?> Verify(E record)
    {
        MigrationLog? log = await Config
            .Migrator
            .MigrationLogs
            .FirstOrDefaultAsync(x =>
                x.OriginId == record.OriginKey
                && x.TargetType == typeof(E).FullName
            );
            
        return log?.Id;
    }
}