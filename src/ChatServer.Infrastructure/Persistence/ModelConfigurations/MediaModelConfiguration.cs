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

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.HasOne<Client>()
            .WithMany(x => x.Media)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
