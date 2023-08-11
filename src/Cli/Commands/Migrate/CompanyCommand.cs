using Cli.Seeders;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class CompanyCommand : SeederCommand<CompanySeeder, Company>
{
    public CompanyCommand() : base(
        "Seed Companies to Target schema"
    )
    { }
}