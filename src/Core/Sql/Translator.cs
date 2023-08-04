using System.Text;
using Core.Data;
using Core.Schema;
using Microsoft.EntityFrameworkCore;

namespace Core.Sql;
public abstract class Translator<T> where T : IMigrationTarget
{
    public TranslatorConfig Config { get; }

    public Translator(
      string table,
      string source = "Origin",
      string target = "Target",
      string migrator = "Migration"
    )
    {
        Config = new(table, typeof(T).Name, source, target, migrator);
    }

    protected virtual Func<T, Task<T>>? OnMigrate { get; set; }

    protected abstract T ToV1Null();
    protected abstract string[] InsertProps();
    protected abstract Task<T?> GetByKey(string key);
    public abstract Task<List<T>> Get();

    public async Task<List<T>> Migrate()
    {
        List<T> data = await Get();
        return await InsertMany(data);
    }

    protected string InsertQuery()
    {
        StringBuilder query = new();

        query.AppendLine($"INSERT INTO {Config.Table} (");

        query.Append(
            $"  {string.Join(",\r\n  ", InsertProps().Select(x => $"[{x.Trim().Trim(',')}]"))}\r\n"
        );

        query.AppendLine(")");
        query.AppendLine("OUTPUT INSERTED.* ");
        query.AppendLine("VALUES(");

        query.Append(
            $"  {string.Join(",\r\n  ", InsertProps().Select(x => $"@{x.Trim().Trim(',')}"))}\r\n"
        );

        query.Append(')');

        return query.ToString();
    }

    public async Task<T?> Insert(T data)
    {
        bool verified = await Verify(data.SourceId);

        if (verified)
        {
            if (OnMigrate is not null)
                data = await OnMigrate(data);

            T result = await Config.Target.Insert(InsertQuery(), data);
            await MigrateLog(data.SourceId, result.Id, result.Type);
            return result;
        }
        else
            return default;
    }

    public async Task<List<T>> InsertMany(List<T> data)
    {
        List<T> results = new();

        foreach (T value in data)
        {
            T? result = await Insert(value);

            if (result is not null)
                results.Add(result);
        }

        return results;
    }

    /*
      Given the V1 key for a record, ensure that the
      corresponding record has been migrated.
      Returns the V2 key for the migrated record.
    */
    public async Task<int> EnsureMigrated(string key)
    {
        int id = await GetTargetIdFromKey(key);

        if (id < 1)
        {
            T? entity = (key == "V1Null"
                ? ToV1Null()
                : await GetByKey(key))
            ?? throw new Exception($"Provided key {key} references a non-existent {Config.Entity}");

            entity = await Insert(entity);

            if (entity is null)
                throw new Exception($"An error occurred migrating the provided {Config.Entity}");

            id = entity.Id;
        }

        return id;
    }

    protected async Task<int> GetTargetIdFromKey(string key) =>
        await Config
            .Migrator
            .MigrationLogs
            .Where(x =>
                x.OriginId == key
                && x.TargetType == typeof(T).FullName
            )
            .Select(x => x.TargetId)
            .FirstOrDefaultAsync();

    protected async Task<bool> Verify(string key) =>
        !await Config
            .Migrator
            .MigrationLogs
            .AnyAsync(x =>
                x.OriginId == key
                && x.TargetType == typeof(T).FullName
            );

    protected async Task MigrateLog(string originId, int targetId, string targetType)
    {
        Console.WriteLine($"Logging {targetType} Migration: {originId} => {targetId}");

        await Config.Migrator.MigrationLogs.AddAsync(new MigrationLog()
        {
            OriginId = originId,
            TargetId = targetId,
            TargetType = targetType
        });

        await Config.Migrator.SaveChangesAsync();
    }
}