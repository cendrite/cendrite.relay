using Cypherly.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cypherly.Persistence.ModelConfigurations;

public class OutboxMessageModelConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.ProcessedOn);

        builder.Property(x => x.Error);
    }
}