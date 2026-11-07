namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record DeviceAddedEvent
{
    public required Guid DeviceId { get; init; }
    public required Guid ConnectionId { get; init; }
    public required string DevicePlatform { get; init; }
}
