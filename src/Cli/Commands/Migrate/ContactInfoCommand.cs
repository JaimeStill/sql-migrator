using Cli.Translators;
using Core.Schema.AdventureWorks;

namespace Cli.Commands;
public class ContactInfoCommand : TranslatorCommand<ContactInfoTranslator, ContactInfo>
{
    public ContactInfoCommand() : base(
        "Migrate Contact Info from AdventureWorks to Target schema"
    )
    { }
}