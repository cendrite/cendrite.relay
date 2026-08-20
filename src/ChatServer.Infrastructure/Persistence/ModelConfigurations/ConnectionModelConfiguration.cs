using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Infrastructure.Persistence.ModelConfigurations;

public class ConnectionModelConfiguration : BaseEntityTypeConfiguration<Connection>
{
    public override void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("connection");

        builder.Ignore(e => e.Id);

        builder.HasKey(nameof(Connection.Type), nameof(Connection.ClientId))
            .HasName("id");

        builder.Property(x => x.TransientId)
            .HasColumnName("transient_id");
    }
}