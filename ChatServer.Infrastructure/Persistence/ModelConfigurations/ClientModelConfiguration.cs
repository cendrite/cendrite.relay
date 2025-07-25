using ChatServer.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ClientModelConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConnectionId)
            .IsRequired();

        builder.HasIndex(x => x.ConnectionId)
            .IsUnique();

        // Only relevant for caching purposes in relation to SignalR connections
        builder.Ignore(x => x.TransientId);

        builder.HasMany(x => x.ChatMessages)
            .WithOne()
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}