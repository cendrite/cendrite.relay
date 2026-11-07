namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record AccountKeyRotatedEvent
{
    public required Guid DeviceId { get; init; }
    public required string DeviceName { get; init; }
}
