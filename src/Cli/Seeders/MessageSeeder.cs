using Core.Schema.AdventureWorks;

namespace Cli.Seeders;
public class MessageSeeder : AwSeeder<Message>
{
    readonly Employee employee;

    public MessageSeeder(
        Employee employee,
        string target = "Target",
        string migrator = "Migration"
    ) : base(
        target,
        migrator
    )
    {
        this.employee = employee;
    }

    protected override Func<Message, Task<Message>>? OnInsert => (Message message) =>
    {
        message.RecipientId = employee.Id;
        return Task.FromResult(message);
    };

    public override List<Message> Records => new()
    {
        new()
        {
            Title = "Welcome to AdventureWorks",
            Body = "Your data has been successfully migrated to the new application!"
        }
    };

    protected override string[] FindCommands(Message record)
    {
        List<string> commands = new()
        {
            $"WHERE [e].[RecipientId] = '{record.RecipientId}'",
            $"AND [e].[Title] = '{record.Title}'",
            $"AND [e].[Body] = '{record.Body}'"
        };

        if (record.SenderId is not null)
            commands.Add($"AND [e].[SenderId] = '{record.SenderId.Value}'");

        return commands.ToArray();
    }

    protected override string[] InsertProps() =>
        InsertProps(new string[] {
            "RecipientId",
            "SenderId",
            "Body",
            "Title"
        });
}