using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.ModelConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MediaModelConfiguration : BaseEntityTypeConfiguration<Media>
{
    public override void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("media");

        builder.Ignore(e => e.Id);

        builder.HasKey(x => x.MediaId)
            .HasName("media_id");

        builder.Property(x => x.MediaId)
            .HasColumnName("media_id")
            .IsRequired();
        
        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.HasOne<Client>()
            .WithMany(x => x.Media)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
