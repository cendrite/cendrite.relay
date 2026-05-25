namespace ChatServer.Application.Features.ChangeEvent.Event;

public sealed record UserLoggedOutEvent
{
    public required Guid ConnectionId { get; init; }
    public required string UserTag { get; init; }
}
