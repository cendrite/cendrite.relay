namespace ChatServer.Application.Features.Client.Queries.GetPresence;

public sealed record GetPresenceDto
{
    public required IReadOnlyCollection<DevicePresenceDto> Devices { get; init; } = [];

    public static GetPresenceDto Map(IEnumerable<Domain.Aggregates.Client> clients) => new()
    {
        Devices = [.. clients.Select(c => new DevicePresenceDto { DeviceId = c.Id, Online = c.IsOnline() })],
    };
}

public sealed record DevicePresenceDto
{
    public required Guid DeviceId { get; init; }
    public required bool Online { get; init; }
}
