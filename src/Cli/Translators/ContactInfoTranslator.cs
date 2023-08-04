using System.Text;
using Core.Schema.AdventureWorks;
using Core.Sql;

namespace Cli.Translators;
public class ContactInfoTranslator : Translator<ContactInfo>
{
    readonly EmployeeTranslator employeeTranslator;

    public ContactInfoTranslator(
        string source = "origin",
        string target = "target",
        string migrator = "migration"
    ) : base(
        typeof(ContactInfo).Name,
        source,
        target,
        migrator
    )
    {
        employeeTranslator = new(source, target, migrator);
    }

    protected override Func<ContactInfo, Task<ContactInfo>>? OnMigrate => async (info) =>
    {
        info.EmployeeId = await employeeTranslator.EnsureMigrated(info.SourceEmployeeId);
        return info;
    };

    protected override string[] InsertProps() => new string[] {
        "EmployeeId",
        "Type",
        "ContactType",
        "Value"
    };

    protected override ContactInfo ToV1Null() => new()
    {
        SourceEmployeeId = "V1Null",
        Value = "V1Null",
        ContactType = "V1Null"
    };

    protected static string[] GetPhoneQuery() => new string[] {
        "SELECT",
        "  CAST([person].[BusinessEntityID] as nvarchar(MAX)) [SourceEmployeeId],",
        "  CAST([phone].[PhoneNumber] as nvarchar(MAX)) [Value],",
        "  CAST([phoneType].[Name] as nvarchar(MAX)) [ContactType]",
        "FROM [Person].[Person] [person]",
        "LEFT JOIN [Person].[PersonPhone] [phone]",
        "ON [person].[BusinessEntityID] = [phone].[BusinessEntityID]",
        "LEFT JOIN [Person].[PhoneNumberType] [phoneType]",
        "ON [phone].[PhoneNumberTypeID] = [phoneType].[PhoneNumberTypeID]",
        "WHERE [person].[PersonType] = 'EM'"
    };

    protected static string[] GetEmailQuery() => new string[] {
        "SELECT",
        "  CAST([person].[BusinessEntityID] as nvarchar(MAX)) [SourceEmployeeId],",
        "  CAST([email].[EmailAddress] as nvarchar(MAX)) [Value],",
        "  CAST('Email' as nvarchar(MAX)) [ContactType]",
        "FROM [Person].[Person] [person]",
        "LEFT JOIN [Person].[EmailAddress] [email]",
        "ON [person].[BusinessEntityID] = [email].[BusinessEntityID]",
        "WHERE [person].[PersonType] = 'EM'"
    };

    protected override async Task<ContactInfo?> GetByKey(string key)
    {
        string[] split = key.Split('.');
        string employeeId = split[0];
        string contactType = split[1];
        string value = split[2];
        
        StringBuilder query = new();

        switch (contactType)
        {
            case "Email":
                foreach (string line in GetEmailQuery())
                    query.AppendLine(line);

                query.AppendLine($"AND [email].[EmailAddress] = '{value}'");
                break;
            default:
                foreach (string line in GetPhoneQuery())
                    query.AppendLine(line);

                query.AppendLine($"AND [phone].[PhoneNumber] = '{value}'");
                break;
        }

        query.AppendLine($"AND [person].[BusinessEntityID] = `{employeeId}'");

        return await Config
            .Source
            .QueryFirstOrDefault<ContactInfo>(
                query.ToString()
            );
    }

    protected static string GetQuery()
    {
        StringBuilder query = new();

        query.AppendLine("(");
        
        foreach (string line in GetPhoneQuery())
            query.AppendLine($"  {line}");

        query.AppendLine(")");
        query.AppendLine("UNION");
        query.AppendLine("(");

        foreach (string line in GetEmailQuery())
            query.AppendLine($"  {line}");

        query.AppendLine(")");
        query.AppendLine("ORDER BY [Value]");

        return query.ToString();
    }

    public override async Task<List<ContactInfo>> Get()
    {
        string query = GetQuery();

        return await Config
            .Source
            .Query<ContactInfo>(
                query.ToString()
            );
    }
}