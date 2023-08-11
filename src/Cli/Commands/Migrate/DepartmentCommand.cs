using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class DepartmentCommand : TranslatorCommand<DepartmentTranslator, Department>
{
    public DepartmentCommand() : base(
        "Migrate Department from AdventureWorks to Target schema"
    )
    { }
}