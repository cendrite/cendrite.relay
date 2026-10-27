using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Client.Queries.IsRevoked;

public sealed record IsClientRevokedQuery : IQuery<bool>
{
    public required Guid ClientId { get; init; }
}
