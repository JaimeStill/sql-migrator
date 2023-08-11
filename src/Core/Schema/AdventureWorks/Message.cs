using AppMessage = App.Schema.Message;

namespace Core.Schema.AdventureWorks;
public class Message : AppMessage, IMigrationTarget
{
    public string OriginKey { get; set; } = string.Empty;
}