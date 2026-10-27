namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record ClientDeletedEvent(Guid ConnectionId)
{
    public Guid ConnectionId { get; private init; } = ConnectionId;
}
