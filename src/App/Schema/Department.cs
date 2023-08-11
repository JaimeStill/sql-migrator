using System.ComponentModel.DataAnnotations.Schema;
using App.Models;

namespace App.Schema;

[Table("Department")]
public class Department : Entity
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;

    public Company? Company { get; set; }

    public ICollection<Employee> Employees { get; } = new List<Employee>();

    public override Task<ValidationResult> Validate()
    {
        ValidationResult result = new();

        if (string.IsNullOrWhiteSpace(Name))
            result.AddMessage("Department must have a Name");

        if (string.IsNullOrWhiteSpace(Section))
            result.AddMessage("Department must have a Section");

        return Task.FromResult(result);
    }
}