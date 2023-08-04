namespace Core.Schema;
public interface IMigrationTarget
{
    public int Id { get; }
    public string Type { get; }
    public string SourceId { get; }
}