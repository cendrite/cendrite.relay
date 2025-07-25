using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Events.Client;

public sealed record ClientConnectedEvent(Guid ClientId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}