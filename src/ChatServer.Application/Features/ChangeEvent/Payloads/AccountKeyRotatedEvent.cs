namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record AccountKeyRotatedEvent
{
    public required Guid ConnectionId { get; init; }
    public required string DeviceName { get; init; }
}
