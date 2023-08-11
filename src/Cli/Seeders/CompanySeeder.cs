using Core.Schema.AdventureWorks;

namespace Cli.Seeders;
public class CompanySeeder : AwSeeder<Company>
{
    public CompanySeeder(
        string target = "Target",
        string migrator = "Migration"
    ) : base(
        target,
        migrator
    )
    { }

    public override List<Company> Records => new()
    {
        new()
        {
            Name = "AdventureWorks"
        }
    };

    protected override string[] FindCommands(Company record) => new string[] {
        $"WHERE [e].[Name] = '{record.Name}'"
    };

    protected override string[] InsertProps() =>
        InsertProps(new string[] {
            "Name"
        });
}