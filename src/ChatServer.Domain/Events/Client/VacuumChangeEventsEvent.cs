using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Events.Client;

public sealed record VacuumChangeEventsEvent(Guid ConnectionId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
