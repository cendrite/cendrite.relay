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
    : ICommandHandler<CreateMediaCommand, string>
{
    public async ValueTask<Result<string>> Handle(CreateMediaCommand cmd, CancellationToken ct)
    {
        var client = await clRepo.GetAsync(new ClientWithMediaSpec(cmd.ClientId), ct);

        if (client is null)
        {
            logger.LogCritical("Client with ID {ClientId} not found.", cmd.ClientId);
            return Result.Fail<string>(Error.NotFound<Domain.Aggregates.Client>(cmd.ClientId.ToString()));
        }

        if (client.GetTotalMediaSize() + cmd.Size > 1024L * 1024 * 1024)
        {
            logger.LogWarning("Client with ID {ClientId} exceeded media size limit.", cmd.ClientId);
            return Result.Fail<string>(Error.Validation("Media size limit exceeded."));
        }

        var media = new Domain.Entities.Media(Guid.NewGuid().ToString(), cmd.ClientId, client.ConnectionId, cmd.Size);

        client.AddMedia(media);

        await uow.SaveChangesAsync(ct);

        return Result.Ok(media.MediaId);
    }
}
