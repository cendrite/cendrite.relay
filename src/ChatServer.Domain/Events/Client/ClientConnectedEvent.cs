using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Events.Client;

public sealed record ClientConnectedEvent(Guid ClientId, Guid ConnectionId, string TransientId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
