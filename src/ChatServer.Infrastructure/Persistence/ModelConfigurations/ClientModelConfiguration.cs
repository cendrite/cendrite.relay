using ChatServer.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ClientModelConfiguration : BaseEntityTypeConfiguration<Client>
{
    public override void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("client");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.ConnectionId)
            .HasColumnName("connection_id")
            .IsRequired();

        builder.HasIndex(x => x.ConnectionId)
            .HasDatabaseName("idx_client_connection_id")
            .IsUnique();

        builder.Ignore(x => x.TransientId);

        builder.HasMany(x => x.ChatMessages)
            .WithOne()
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Media)
            .WithOne()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);  
        
        base.Configure(builder);
    }
}
