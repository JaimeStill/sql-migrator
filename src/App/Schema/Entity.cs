using App.Models;

namespace App.Schema;
public abstract class Entity
{
    public int Id { get; set; }
    public string Type { get; private set; }

    public Entity()
    {
        Type = GetType().FullName ?? "Entity";
    }

    public abstract Task<ValidationResult> Validate();
}