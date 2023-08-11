using Core.Schema;

namespace Core.Sql;
public record InsertResult<T>
where T : IMigrationTarget
{
    public bool Inserted { get; set; }
    public T Data { get; set; }

    public InsertResult(T data, bool inserted = false)
    {
        Data = data;
        Inserted = inserted;
    }
}