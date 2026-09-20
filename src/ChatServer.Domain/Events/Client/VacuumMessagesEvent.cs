using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Events.Client;

public sealed record VacuumMessagesEvent(Guid ConnectionId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
