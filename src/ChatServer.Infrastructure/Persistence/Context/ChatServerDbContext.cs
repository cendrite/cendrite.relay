using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using Cendrite.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Context;

public sealed class ChatServerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessage { get; init; }
    public DbSet<Client> Client { get; init; }
    public DbSet<Message> ChatMessage { get; init; }
    public DbSet<Media> Media { get; init; }
    public DbSet<Event> Event { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatServerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
