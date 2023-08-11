using App.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Config;
public class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder
            .HasOne(x => x.Sender)
            .WithMany(x => x.Outbox)
            .HasForeignKey(x => x.SenderId)
            .IsRequired(false);

        builder
            .HasOne(x => x.Recipient)
            .WithMany(x => x.Inbox)
            .HasForeignKey(x => x.RecipientId)
            .IsRequired();
    }
}