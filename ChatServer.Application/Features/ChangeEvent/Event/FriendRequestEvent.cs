namespace ChatServer.Application.Features.ChangeEvent.Event;

public class FriendRequestEvent
{
    public required string Tag { get; init; }
    public required string Username { get; init; }
    public required DateTime RequestedAt { get; init; }
    public bool IsSeen { get; init; }
    public string? DisplayName { get; set; }
    public Guid? AvatarKey { get; set; }
}
