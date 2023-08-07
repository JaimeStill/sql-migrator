namespace App.Models;
public class ValidationResult
{
    List<string> Messages { get; set; }
    public string Message => string.Join('\n', Messages);
    public bool IsValid => Messages.Count < 1;

    public ValidationResult()
    {
        Messages = new List<string>();
    }

    public ValidationResult(string message)
    {
        Messages = new()
        {
            message
        };
    }

    public ValidationResult(ValidationResult parent)
    {
        Messages = parent.Messages;
    }

    public ValidationResult(ValidationResult a, ValidationResult b)
    {
        Messages = a.Messages
            .Union(b.Messages)
            .ToList();
    }

    public void AddMessage(string message) =>
        Messages.Add(message);
}