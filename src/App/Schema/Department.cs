using System.ComponentModel.DataAnnotations.Schema;

namespace App.Schema;

[Table("Department")]
public class Department : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; } = new List<Employee>();
}