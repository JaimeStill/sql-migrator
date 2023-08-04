using AppEmployee = App.Schema.Employee;

namespace Core.Schema.AdventureWorks;
public class Employee : AppEmployee, IMigrationTarget
{
    public string SourceId { get; set; } = string.Empty;
    public string SourceDepartmentId { get; set; } = string.Empty;
}