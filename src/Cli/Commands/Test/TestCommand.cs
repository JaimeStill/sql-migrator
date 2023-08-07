namespace Cli.Commands;
public class TestCommand : CliCommand
{
    public TestCommand() : base(
        "test",
        "Test out command infrastructure",
        commands: new()
        {
            new ConnectorCommand(),
            new ContextBuilderCommand()
        }
    )
    { }
}