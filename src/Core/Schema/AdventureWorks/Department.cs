using AppDepartment = App.Schema.Department;

namespace Core.Schema.AdventureWorks;
public class Department : AppDepartment, IMigrationTarget
{
    public string OriginKey { get; set; } = string.Empty;
}