namespace ChatServer.Application.Features.ChangeEvent.Event;

public sealed record FriendDeletedEvent
{
    public required string Tag { get; init; }
    public required string DeletedBy { get; init; }
}