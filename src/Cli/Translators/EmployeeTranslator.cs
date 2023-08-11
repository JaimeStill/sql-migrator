using Cli.Seeders;
using Core.Schema.AdventureWorks;

namespace Cli.Translators;
public class EmployeeTranslator : AwTranslator<Employee>
{
    readonly DepartmentTranslator departmentTranslator;

    public EmployeeTranslator(
        string source = "origin",
        string target = "target",
        string migrator = "migration"
    ) : base(
        source,
        target,
        migrator
    )
    {
        departmentTranslator = new(source, target, migrator);
    }

    protected override Func<Employee, Task<Employee>>? OnMigrate => async (Employee employee) =>
    {
        employee.DepartmentId = await departmentTranslator.EnsureMigrated(employee.OriginDepartmentKey);
        return employee;
    };

    protected override Func<Employee, Task>? AfterMigrate => async (Employee record) =>
    {
        MessageSeeder seeder = new(record, Config.TargetKey, Config.MigratorKey);
        await seeder.Migrate();
    };

    protected override string[] GetProps() => new string[] {
        "CAST([person].[BusinessEntityID] as nvarchar(MAX)) [OriginKey],",
        "CAST([history].[DepartmentID] as nvarchar(MAX)) [OriginDepartmentKey],",
        "[employee].[NationalIdNumber] [NationalId],",
        "[person].[LastName] [LastName],",
        "[person].[FirstName] [FirstName],",
        "[person].[MiddleName] [MiddleName],",
        "[employee].[LoginId] [Login],",
        "[employee].[JobTitle] [JobTitle]"
    };

    protected override string[] RootCommands() => new string[] {
        "FROM [Person].[person] [person]",
        "LEFT JOIN [HumanResources].[Employee] [employee]",
        "ON [person].[BusinessEntityID] = [employee].[BusinessEntityID]",
        "LEFT JOIN [HumanResources].[EmployeeDepartmentHistory] [history]",
        "ON [employee].[BusinessEntityID] = [history].[BusinessEntityID]",
        "WHERE [person].[PersonType] = 'EM'",
        "AND [history].[EndDate] IS NULL"
    };

    protected override string[] InsertProps() => InsertProps(
        new string[] {
            "DepartmentId",
            "NationalId",
            "LastName",
            "FirstName",
            "MiddleName",
            "Login",
            "JobTitle"
        }
    );

    protected override Employee ToV1Null() => new()
    {
        OriginKey = "V1Null",
        OriginDepartmentKey = "V1Null",
        LastName = "V1Null"
    };

    protected override async Task<Employee?> GetByKey(string key)
    {
        string[] query = BuildCommands(new string[] {
            $"AND [person].[BusinessEntityID] = '{key}'"
        });

        return await GetByKey(
            query
        );
    }

    public override async Task<List<Employee>> Get() =>
        await Get(RootCommands());
}