using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;

namespace ChatServer.Application.Features.Client.Queries.IsRevoked;

public sealed class IsClientRevokedQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<IsClientRevokedQuery, bool>
{
    public async ValueTask<Result<bool>> Handle(IsClientRevokedQuery query, CancellationToken ct)
    {
        var client = await clientRepository.GetAsync(new ClientByIdSpec(query.ClientId), ct);

        return Result.Ok(client is null || client.Deleted is not null);
    }
}
