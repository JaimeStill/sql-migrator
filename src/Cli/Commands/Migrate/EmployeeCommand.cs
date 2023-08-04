using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class EmployeeCommand : TranslatorCommand<EmployeeTranslator, Employee>
{
    public EmployeeCommand() : base(
        "Migrate Employee from AdventureWorks to V2"
    )
    { }
}