using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Client.Queries.GetPresence;

public sealed record GetPresenceQuery : IQuery<GetPresenceDto>
{
    public required Guid UserId { get; init; }
}
