using System.ComponentModel.DataAnnotations.Schema;

namespace App.Schema;

[Table("ContactInfo")]
public class ContactInfo : Entity
{
    public int EmployeeId { get; set; }
    public string ContactType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Employee? Employee { get; set; }
}