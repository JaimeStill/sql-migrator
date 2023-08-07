using System.ComponentModel.DataAnnotations.Schema;
using App.Models;

namespace App.Schema;

[Table("ContactInfo")]
public class ContactInfo : Entity
{
    public int EmployeeId { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Employee? Employee { get; set; }

    public override Task<ValidationResult> Validate()
    {
        ValidationResult result = new();

        if (EmployeeId < 1)
            result.AddMessage("Contact Info must be associated with an Employee");

        if (string.IsNullOrWhiteSpace(ContactType))
            result.AddMessage("Contact Info must have a Contact Type");

        if (string.IsNullOrWhiteSpace(Value))
            result.AddMessage("Contact Info must have a Value");

        return Task.FromResult(result);
    }
}