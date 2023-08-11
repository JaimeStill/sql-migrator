using AppContactInfo = App.Schema.ContactInfo;

namespace Core.Schema.AdventureWorks;
public class ContactInfo : AppContactInfo, IMigrationTarget
{
    public string OriginKey => $"{OriginEmployeeKey}.{ContactType}.{Value}";
    public string OriginEmployeeKey { get; set; } = string.Empty;
}