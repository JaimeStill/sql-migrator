using Core.Schema.AdventureWorks;
using Core.Sql;

namespace Cli.Commands;
public class ConnectorCommand : CliCommand
{
    public ConnectorCommand() : base(
        "connector",
        "Test out connecting with a Dapper SQL Connector",
        new Func<Task>(Call)
    )
    { }

    static async Task Call()
    {
        string sql = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "department-schema.sql");
        string query = await File.ReadAllTextAsync(sql);

        Connector connector = new("Origin");
        List<Department> departments = await connector.Query<Department>(query);

        departments.ForEach(d => Console.WriteLine($"{d.Name} - {d.Section}"));
    }
}