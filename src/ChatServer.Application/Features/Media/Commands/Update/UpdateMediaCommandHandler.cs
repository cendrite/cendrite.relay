using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Media.Commands.Update;

public sealed class UpdateMediaCommandHandler(
    IClientRepository clRepo,
    IUnitOfWork uow,
    ILogger<UpdateMediaCommandHandler> logger) 
    : ICommandHandler<UpdateMediaCommand>
{
    public async Task<Result> Handle(UpdateMediaCommand cmd, CancellationToken ct)
    {
        var client = await clRepo.GetAsync(new ClientByConnectionIdWithMediaSepc(cmd.SenderId), ct);
        if (client is null)
        {
            logger.LogWarning("Client with connection id {ConnectionId} not found", cmd.SenderId);
            return Result.Fail(Error.NotFound<Domain.Aggregates.Client>(cmd.SenderId.ToString()));
        }

        var media = client.GetMedia(cmd.MediaId);
        if (media is null)
        {
            logger.LogWarning("Media with id {MediaId} not found for client {ClientId}", cmd.MediaId, client.Id);
            return Result.Fail(Error.NotFound<Domain.Entities.Media>(cmd.MediaId));
        }

        media.MarkAsSent();
        await uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
