using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Events.Client
{
    public sealed record ClientLogoutEvent : IDomainEvent
    {
        public Guid ClientId { get; init; }
        public DateTime OccurredOn { get; init; }
    }
}
