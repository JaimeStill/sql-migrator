using System.ComponentModel.DataAnnotations.Schema;
using App.Models;

namespace App.Schema;

[Table("Employee")]
public class Employee : Entity
{
    public int DepartmentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public Department? Department { get; set; }

    public ICollection<ContactInfo> ContactInfo { get; } = new List<ContactInfo>();
    public ICollection<Message> Inbox { get; } = new List<Message>();
    public ICollection<Message> Outbox { get; } = new List<Message>();

    public override Task<ValidationResult> Validate()
    {
        ValidationResult result = new();

        if (DepartmentId < 1)
            result.AddMessage("Employee must be assigned to a Department");

        if (string.IsNullOrWhiteSpace(FirstName))
            result.AddMessage("Employee must have a First Name");

        if (string.IsNullOrWhiteSpace(LastName))
            result.AddMessage("Employee must have a Last Name");

        if (string.IsNullOrWhiteSpace(JobTitle))
            result.AddMessage("Employee must have a Job Title");

        if (string.IsNullOrWhiteSpace(NationalId))
            result.AddMessage("Employee must have a National ID");

        if (string.IsNullOrWhiteSpace(Login))
            result.AddMessage("Employee must have a Login");

        return Task.FromResult(result);
    }
}