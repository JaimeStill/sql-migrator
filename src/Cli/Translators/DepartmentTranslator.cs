using Core.Schema.AdventureWorks;

namespace Cli.Translators;
public class DepartmentTranslator : AwTranslator<Department>
{
    public DepartmentTranslator(
        string source = "Origin",
        string target = "Target",
        string migrator = "Migration"
    ) : base(
        source,
        target,
        migrator
    )
    { }

    protected override string[] GetProps() => new string[] {
        "CAST([department].[DepartmentID] as nvarchar(MAX)) [SourceId],",
        "[department].[Name] [Name],",
        "[department].[GroupName] [Section]"
    };

    protected override string[] RootCommands() => new string[] {
        "FROM [HumanResources].[Department] [department]"
    };

    protected override string[] InsertProps() =>
        InsertProps(new string[] {
            "Name",
            "Section"
        });

    protected override Department ToV1Null() => new()
    {
        SourceId = "V1Null",
        Name = "V1Null"
    };

    protected override async Task<Department?> GetByKey(string key) =>
        await GetByKey(
            BuildCommands(new string[] {
                $"WHERE [department].[DepartmentID] = '{key}'"
            })
        );

    public override async Task<List<Department>> Get() =>
        await Get(RootCommands());
}