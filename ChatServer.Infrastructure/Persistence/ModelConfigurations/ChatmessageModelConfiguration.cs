using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ChatmessageModelConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("Chatmessage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderId)
            .IsRequired();

        builder.Property(x => x.RecipientId)
            .IsRequired();

        builder.Property(x => x.ChatId)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}