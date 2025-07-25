namespace ChatServer.Application.Features.ChangeEvent.Event;

public class FriendRequestAcceptedEvent
{
    public required string Username { get; init; }
    public required string Tag { get; init; }
    public string? DisplayName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public IReadOnlyCollection<Guid> ConnectionIds { get; init; } = new List<Guid>();
}