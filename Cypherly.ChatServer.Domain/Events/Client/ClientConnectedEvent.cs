using Cypherly.ChatServer.Domain.Abstractions;

namespace Cypherly.ChatServer.Domain.Events.Client;

public sealed record ClientConnectedEvent(Guid ClientId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}