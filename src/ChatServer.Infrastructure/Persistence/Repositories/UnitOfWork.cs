using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Abstractions;
using ChatServer.Infrastructure.Persistence.Context;
using Cendrite.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatServer.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(ChatServerDbContext context) : IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the context to the database.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages();
        UpdateAuditableEntities();
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Converts domain events of all <see cref="AggregateRoot"/> entities tracked by the context into
    /// outbox messages and persists them to the database.
    /// </summary>
    private void ConvertDomainEventsToOutboxMessages()
    {
        var outboxMessages = context.ChangeTracker.Entries<AggregateRoot>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Any())
            .SelectMany(aggregateRoot =>
            {
                var domainEvents = aggregateRoot.DomainEvents.ToList();
                aggregateRoot.ClearDomainEvents();
                return domainEvents;
            }).Select(domainEvent => new OutboxMessage()
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Type = domainEvent.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException("The domain event type must have an assembly qualified name"),
                Content = JsonConvert.SerializeObject(domainEvent,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
            }).ToList();

        if (outboxMessages.Count != 0)
            context.Set<OutboxMessage>().AddRange(outboxMessages);
    }

    /// <summary>
    /// Updates the Created and Updated timestamps for auditable entities.
    /// </summary>
    private void UpdateAuditableEntities()
    {
        var entities = context.ChangeTracker.Entries<Entity>().Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entity in entities)
        {
            if (entity.State is EntityState.Added)
            {
                entity.Entity.SetCreated();
            }

            entity.Entity.SetLastModified();
        }
    }
}
