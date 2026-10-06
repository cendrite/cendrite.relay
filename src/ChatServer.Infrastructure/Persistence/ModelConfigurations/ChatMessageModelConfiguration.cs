using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ChatMessageModelConfiguration : BaseEntityTypeConfiguration<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("message");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(x => x.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.Property(x => x.MessageToken)
            .HasColumnName("message_token");

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();
        
        builder.Property(x=> x.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();
        
        base.Configure(builder);
    }   
}
