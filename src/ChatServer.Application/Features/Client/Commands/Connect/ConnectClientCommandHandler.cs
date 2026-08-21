using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Common;
using ChatServer.Domain.Events.Client;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Commands.Connect;

public sealed class ConnectClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<ConnectClientCommandHandler> logger)
    : ICommandHandler<ConnectClientCommand>
{
    public async Task<Result> Handle(ConnectClientCommand cmd, CancellationToken ct)
    {
        try
        {
            var client = await clientRepository.GetByIdAsync(cmd.ClientId);

            if (client is null)
            {
                logger.LogError("Clients with ID: {ID} not found in database", cmd.ClientId);
                return Result.Fail(Error.Unauthorized());
            }

            client.AddConnection(new Connection(cmd.TransientId, cmd.ClientId, cmd.HubType.ToString()));
            client.AddDomainEvent(new ClientConnectedEvent(cmd.ClientId));

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogCritical("{Exception}. For Clients with ID: {ID}", e, cmd.ClientId);
            return Result.Fail(Error.Failure("ConnectCommandHandler"));
        }
    }
}
