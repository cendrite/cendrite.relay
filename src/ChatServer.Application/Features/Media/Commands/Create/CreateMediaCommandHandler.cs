using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed class CreateMediaCommandHandler(
    IClientRepository clRepo,
    IUnitOfWork uow,
    ILogger<CreateMediaCommandHandler> logger)
    : ICommandHandler<CreateMediaCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMediaCommand cmd, CancellationToken ct)
    {
        var client = await clRepo.GetAsync(new ClientWithMediaSpec(cmd.ClientId), ct);

        if (client is null)
        {
            logger.LogCritical("Client with ID {ClientId} not found.", cmd.ClientId);
            return Result.Fail<Guid>(Error.NotFound<Domain.Aggregates.Client>(cmd.ClientId.ToString()));
        }

        var media = new Domain.Entities.Media(Guid.NewGuid().ToString(), cmd.ClientId, client.ConnectionId);

        client.AddMedia(media);

        await uow.SaveChangesAsync(ct);

        return Result.Ok(media.Id);
    }
}
