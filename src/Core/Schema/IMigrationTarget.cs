namespace Core.Schema;
public interface IMigrationTarget
{
    public int Id { get; set; }
    public string Type { get; }
    public string OriginKey { get; }
}