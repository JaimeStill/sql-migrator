using Core.Schema;

namespace Core.Sql;
public record TranslatorConfig<E> : SequenceConfig<E>
where E : IMigrationTarget
{
    public Connector Source { get; }

    public TranslatorConfig(
        string source,
        string target,
        string migrator
    ) : base(target, migrator)
    {
        Source = new(source);
    }
}