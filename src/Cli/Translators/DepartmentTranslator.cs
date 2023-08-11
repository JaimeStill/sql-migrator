using Cli.Seeders;
using Core.Schema.AdventureWorks;

namespace Cli.Translators;
public class DepartmentTranslator : AwTranslator<Department>
{
    readonly CompanySeeder companySeeder;

    public DepartmentTranslator(
        string source = "Origin",
        string target = "Target",
        string migrator = "Migration"
    ) : base(
        source,
        target,
        migrator
    )
    {
        companySeeder = new(target, migrator);
    }

    protected override Func<Department, Task<Department>>? OnInsert => async (Department department) =>
    {
        department.CompanyId = await companySeeder.EnsureMigrated(companySeeder.Default);
        return department;
    };

    protected override string[] GetProps() => new string[] {
        "CAST([department].[DepartmentID] as nvarchar(MAX)) [OriginKey],",
        "[department].[Name] [Name],",
        "[department].[GroupName] [Section]"
    };

    protected override string[] RootCommands() => new string[] {
        "FROM [HumanResources].[Department] [department]"
    };

    protected override string[] InsertProps() =>
        InsertProps(new string[] {
            "CompanyId",
            "Name",
            "Section"
        });

    protected override Department ToV1Null() => new()
    {
        OriginKey = "V1Null",
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