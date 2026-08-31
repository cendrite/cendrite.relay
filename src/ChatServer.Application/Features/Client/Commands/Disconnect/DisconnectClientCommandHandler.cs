using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Commands.Disconnect;

public class DisconnectClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<DisconnectClientCommandHandler> logger)
    : ICommandHandler<DisconnectClientCommand>
{
    public async Task<Result> Handle(DisconnectClientCommand cmd, CancellationToken ct)
    {
        try
        {
            var client = await clientRepository.GetAsync(new ClientByTransientIdSpec(cmd.TransientId), ct);

            if (client is null)
            {
                logger.LogWarning("Clients with Transient ID: {ID} not found in cache", cmd.TransientId);
                return Result.Fail(Error.NotFound<Domain.Aggregates.Client>(cmd.TransientId));
            }

            client.RemoveConnection(cmd.TransientId);

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogCritical("Exception occurred in DisconnectClientCommandHandler: {Exception}. For Clients with Transient ID: {ID}", e, cmd.TransientId);
            return Result.Fail(Error.Failure("DisconnectClientCommandHandler"));
        }
    }
}
