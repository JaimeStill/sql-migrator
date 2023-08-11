using System.ComponentModel.DataAnnotations.Schema;
using App.Models;

namespace App.Schema;

[Table("Message")]
public class Message : Entity
{
    // system generated if null
    public int? SenderId { get; set; }
    public int RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public Employee? Sender { get; set; }
    public Employee? Recipient { get; set; }

    public override Task<ValidationResult> Validate()
    {
        ValidationResult result = new();

        if (string.IsNullOrWhiteSpace(Title))
            result.AddMessage("Message must have a Title");

        if (string.IsNullOrWhiteSpace(Body))
            result.AddMessage("Message must have a Body");

        return Task.FromResult(result);        
    }
}