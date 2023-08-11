using Core.Schema;
using Core.Sql;

namespace Cli.Seeders;
public abstract class AwSeeder<E> : Seeder<E>
where E : IMigrationTarget
{
    protected static readonly string[] insertProps = {
        "Type"
    };

    public AwSeeder(
        string target = "Target",
        string migrator = "Migration"
    ) : base(target, migrator)
    { }

    public virtual string[] InsertProps(string[] props) =>
        insertProps
            .Union(props)
            .ToArray();
}