using System.CommandLine;

namespace Cli;
public class CliApp
{
    readonly RootCommand root;

    public CliApp(
        string description,
        List<CliCommand> commands,
        List<Option>? globals = null
    )
    {
        root = new(description);

        if (globals?.Count > 0)
            globals.ForEach(root.AddGlobalOption);

        commands
            .Select(x => x.Build())
            .ToList()
            .ForEach(root.AddCommand);
    }

    public Task InvokeAsync(params string[] args) =>
        root.InvokeAsync(args);
}