using ChatServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class EventModelConfiguration : BaseEntityTypeConfiguration<Event>
{
    public override void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("event");

        builder.HasKey(x => new { x.Id, x.RecipientId });

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(x => new { x.RecipientId, x.Created, x.Id })
            .HasDatabaseName("idx_event_recipient_id_created_id");

        base.Configure(builder);
    }
}
