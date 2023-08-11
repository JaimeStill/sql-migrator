using AppCompany = App.Schema.Company;

namespace Core.Schema.AdventureWorks;
public class Company : AppCompany, IMigrationTarget
{
    public string OriginKey { get; set; } = string.Empty;
}