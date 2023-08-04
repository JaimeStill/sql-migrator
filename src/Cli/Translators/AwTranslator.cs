using System.Text;
using Core.Schema;
using Core.Sql;

namespace Cli.Translators;
public abstract class AwTranslator<T> : Translator<T> where T : IMigrationTarget
{
    protected static readonly string[] insertProps = {
        "Type"
    };

    public AwTranslator(
        string source = "Origin",
        string target = "Target",
        string migrator = "Migration"
    ) : base(typeof(T).Name, source, target, migrator)
    { }

    protected abstract string[] RootCommands();
    protected abstract string[] GetProps();

    public string[] InsertProps(string[] props) =>
        insertProps
            .Union(props)
            .ToArray();

    protected string[] BuildCommands(string[] commands) =>
        RootCommands()
            .Concat(commands)
            .ToArray();

    protected string GetQuery(string[] commands, string select)
    {
        StringBuilder query = new();

        query.AppendLine(select);

        foreach (string prop in GetProps())
            query.AppendLine($"  {prop.Trim()}");

        foreach(string command in commands)
            query.AppendLine(command);

        return query.ToString();
    }

    protected async Task<List<T>> Get(string[] commands, string select = "SELECT") =>
        await Config.Source.Query<T>(
            GetQuery(commands, select)
        );

    protected async Task<T?> GetByKey(string[] commands)
    {
        string query = GetQuery(commands, "SELECT TOP(1)");

        return await Config.Source.QueryFirstOrDefault<T>(
            query
        );
    }
}