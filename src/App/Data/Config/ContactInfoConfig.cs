using App.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Config;
public class ContactInfoConfig : IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder
            .HasOne(x => x.Employee)
            .WithMany(x => x.ContactInfo)
            .HasForeignKey(x => x.EmployeeId)
            .IsRequired();
    }
}