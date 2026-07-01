using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications;
using ChatServer.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed class CreateMediaCommandHandler(
    IClientRepository clRepo,
    IUnitOfWork uow,
    ILogger<CreateMediaCommandHandler> logger)
    : ICommandHandler<CreateMediaCommand>
{
    public async Task<Result> Handle(CreateMediaCommand cmd, CancellationToken ct)
    {
        var client = await clRepo.GetAsync(new ClientWithMediaSpec(cmd.ClientId), ct);

        if (client is null)
        {
            logger.LogCritical("Client with ID {ClientId} not found.", cmd.ClientId);
            return Result.Fail(Error.NotFound<Domain.Aggregates.Client>(cmd.ClientId.ToString()));
        }

        var media = new Domain.Entities.Media(cmd.MediaId, cmd.ClientId);

        client.AddMedia(media);

        await uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
