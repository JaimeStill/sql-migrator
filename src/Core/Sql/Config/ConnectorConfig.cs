namespace Core.Sql;
public record ConnectorConfig
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
}