using System.Text;
using Core.Schema;

namespace Core.Sql;
public abstract class Sequence<E, C>
where E : IMigrationTarget
where C : SequenceConfig<E>
{
    public abstract C Config { get; set; }
    public abstract Task<List<E>> Migrate();

    protected abstract string[] InsertProps();
    protected abstract Task<InsertResult<E>> Insert(E record);
    protected abstract Task<int?> Verify(E record);

    protected virtual Func<E, Task<E>>? OnInsert { get; set; }
    protected virtual Func<E, Task<E>>? OnMigrate { get; set; }
    protected virtual Func<E, Task>? AfterMigrate { get; set; }

    protected async Task<List<E>> InsertMany(List<E> records)
    {
        List<E> results = new();

        foreach (E record in records)
        {
            InsertResult<E> result = await Insert(record);

            if (result.Inserted)
                results.Add(result.Data);
        }

        return results;
    }

    protected string InsertQuery()
    {
        StringBuilder query = new();

        query.AppendLine($"INSERT INTO {Config.Entity} (");

        query.Append(
            $"  {string.Join(",\r\n  ", InsertProps().Select(x => $"[{x.Trim().Trim(',')}]"))}\r\n"
        );
        
        query.AppendLine(")");
        query.AppendLine("OUTPUT INSERTED.*");
        query.AppendLine("VALUES(");
        
        query.Append(
            $"  {string.Join(",\r\n  ", InsertProps().Select(x => $"@{x.Trim().Trim(',')}"))}\r\n"
        );
        
        query.Append(')');
        
        return query.ToString();
    }
}