using System.ComponentModel.DataAnnotations.Schema;
using App.Models;

namespace App.Schema;

[Table("Company")]
public class Company : Entity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Department> Departments { get; } = new List<Department>();

    public override Task<ValidationResult> Validate()
    {
        ValidationResult result = new();

        if (string.IsNullOrWhiteSpace(Name))
            result.AddMessage("Company must have a Name");

        return Task.FromResult(result);
    }
}