namespace Core.Data;
public class MigrationLog
{
    public int Id { get; set; }
    public string OriginId { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
}