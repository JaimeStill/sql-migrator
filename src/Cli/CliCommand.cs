using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Cli;
public abstract class CliCommand
{
    readonly string name;
    readonly string description;
    readonly Delegate? @delegate;
    readonly List<Option>? options;
    readonly List<CliCommand>? commands;

    public CliCommand(
        string name,
        string description,
        Delegate? @delegate = null,
        List<Option>? options = null,
        List<CliCommand>? commands = null
    )
    {
        this.name = name;
        this.description = description;
        this.@delegate = @delegate;
        this.options = options;
        this.commands = commands;
    }

    public Command Build()
    {
        Command command = new(name, description);

        if (@delegate is not null)
            command.Handler = CommandHandler.Create(@delegate);

        options?.ForEach(command.AddOption);

        if (commands?.Count > 0)
            commands
              .Select(c => c.Build())
              .ToList()
              .ForEach(command.AddCommand);

        return command;
    }
}