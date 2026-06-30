using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ChatMessageModelConfiguration : BaseEntityTypeConfiguration<ChatMessage>
{
    public override void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_message");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(x => x.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.Property(x => x.ChatId)
            .HasColumnName("chat_id")
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
