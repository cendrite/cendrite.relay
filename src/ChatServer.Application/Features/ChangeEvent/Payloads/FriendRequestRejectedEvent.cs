namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public class FriendRequestRejectedEvent
{
    public required string Tag { get; init; }
}