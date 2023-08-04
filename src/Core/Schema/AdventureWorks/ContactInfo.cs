using AppContactInfo = App.Schema.ContactInfo;

namespace Core.Schema.AdventureWorks;
public class ContactInfo : AppContactInfo, IMigrationTarget
{
    public string SourceId => $"{SourceEmployeeId}.{ContactType}.{Value}";
    public string SourceEmployeeId { get; set; } = string.Empty;
}