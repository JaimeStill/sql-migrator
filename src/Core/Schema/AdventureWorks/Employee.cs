using AppEmployee = App.Schema.Employee;

namespace Core.Schema.AdventureWorks;
public class Employee : AppEmployee, IMigrationTarget
{
    public string OriginKey { get; set; } = string.Empty;
    public string OriginDepartmentKey { get; set; } = string.Empty;
}