using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
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
    public async ValueTask<Result> Handle(ConnectClientCommand cmd, CancellationToken ct)
    {
        try
        {
            var client = await clientRepository.GetByIdAsync(cmd.ClientId);

            if (client is null)
            {
                logger.LogError("Clients with ID: {ID} not found in database", cmd.ClientId);
                return Result.Fail(Error.Unauthorized());
            }

            if (client.Deleted is not null)
            {
                logger.LogWarning("Revoked client with ID: {ID} attempted to connect", cmd.ClientId);
                return Result.Fail(Error.Revoked());
            }

            client.AddConnection(new Connection(cmd.TransientId, client.Id, cmd.HubType.ToString()));
            client.AddDomainEvent(new ClientConnectedEvent(client.Id, client.ConnectionId, cmd.TransientId));

            if (cmd.HubType is HubType.Message)
                client.AddDomainEvent(new VacuumMessagesEvent(client.ConnectionId));

            if (cmd.HubType is HubType.ChangeEvent)
                client.AddDomainEvent(new VacuumChangeEventsEvent(client.ConnectionId));

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
