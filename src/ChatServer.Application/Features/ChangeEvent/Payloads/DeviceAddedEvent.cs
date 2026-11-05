namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record DeviceAddedEvent
{
    public required Guid ConnectionId { get; init; }
    public required string DevicePlatform { get; init; }
}
