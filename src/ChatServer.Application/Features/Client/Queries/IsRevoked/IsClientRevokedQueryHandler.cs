using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Queries.IsRevoked;

public sealed class IsClientRevokedQueryHandler(
    IClientRepository clientRepository,
    ILogger<IsClientRevokedQueryHandler> logger)
    : IQueryHandler<IsClientRevokedQuery, bool>
{
    public async ValueTask<Result<bool>> Handle(IsClientRevokedQuery query, CancellationToken ct)
    {
        try
        {
            var client = await clientRepository.GetAsync(new ClientByIdSpec(query.ClientId), ct);

            return Result.Ok(client is null || client.Deleted is not null);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to check revocation for client with ID: {ID}", query.ClientId);
            return Result.Fail<bool>(Error.Failure("IsClientRevokedQueryHandler"));
        }
    }
}
