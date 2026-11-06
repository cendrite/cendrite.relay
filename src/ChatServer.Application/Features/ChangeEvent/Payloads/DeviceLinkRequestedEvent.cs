namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record DeviceLinkRequestedEvent
{
    public required Guid PendingLinkId { get; init; }
    public required string DevicePlatform { get; init; }
}
