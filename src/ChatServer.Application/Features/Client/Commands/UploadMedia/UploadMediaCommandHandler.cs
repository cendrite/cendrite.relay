using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications;
using ChatServer.Domain.Common;
using ChatServer.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Commands.UploadMedia;

public sealed class UploadMediaCommandHandler(
    IClientRepository clRepo,
    IUnitOfWork uow,
    ILogger<UploadMediaCommandHandler> logger)
    : ICommandHandler<UploadMediaCommand>
{
    public async Task<Result> Handle(UploadMediaCommand cmd, CancellationToken ct)
    {
        var client = await clRepo.GetAsync(new ClientWithMediaSpec(cmd.ClientId), ct);

        if (client is null)
        {
            logger.LogCritical("Client with ID {ClientId} not found.", cmd.ClientId);
            return Result.Fail(Error.NotFound<Domain.Aggregates.Client>(cmd.ClientId.ToString()));
        }

        var media = new Media(cmd.MediaId, cmd.ClientId);

        client.AddMedia(media);

        await uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
