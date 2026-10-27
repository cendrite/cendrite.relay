using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;

namespace ChatServer.Application.Features.Client.Queries.GetPresence;

public sealed class GetPresenceQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<GetPresenceQuery, GetPresenceDto>
{
    public async ValueTask<Result<GetPresenceDto>> Handle(GetPresenceQuery query, CancellationToken ct)
    {
        var clients = await clientRepository.GetActiveByUserIdAsync(query.UserId, ct);

        return Result.Ok(GetPresenceDto.Map(clients));
    }
}
