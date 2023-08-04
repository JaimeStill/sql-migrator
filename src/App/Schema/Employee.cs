using System.ComponentModel.DataAnnotations.Schema;

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
}