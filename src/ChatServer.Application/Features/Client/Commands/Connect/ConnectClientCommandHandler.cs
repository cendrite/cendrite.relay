using ChatServer.Application.Abstractions;
using ChatServer.Application.Cache.Client;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Domain.Common;
using ChatServer.Domain.Events.Client;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Commands.Connect;

public sealed class ConnectClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    IClientCache clientCache,
    ILogger<ConnectClientCommandHandler> logger)
    : ICommandHandler<ConnectClientCommand>
{
    public async Task<Result> Handle(ConnectClientCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.GetByIdAsync(command.ClientId);

            if (client is null)
            {
                logger.LogError("Clients with ID: {ID} not found in database. During ConnectClientCommand operation", command.ClientId);
                return Result.Fail(Errors.General.Unauthorized());
            }

            var existingClient = await clientCache.GetAsync(client.ConnectionId, cancellationToken);

            if (existingClient is not null)
            {
                var existingTransientId = existingClient.GetTransientId(command.HubType);

                if (existingTransientId is not null)
                {
                    return Result.Fail(Errors.General.UnspecifiedError("Clients already exists with hub type"));
                }

                existingClient.AddTransientId(command.TransientId, command.HubType);
                await clientCache.UpdateAsync(existingClient, cancellationToken);
            }
            else
            {
                var newClient = new ClientCacheDto(client.ConnectionId, new Dictionary<HubType, string>
                {
                    { command.HubType, command.TransientId }
                });
                await clientCache.AddAsync(newClient, cancellationToken);
                client.AddDomainEvent(new ClientConnectedEvent(client.Id));
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogCritical("Exception occurred in ConnectCommandHandler: {Exception}. For Clients with ID: {ID}", e, command.ClientId);
            return Result.Fail(Errors.General.UnspecifiedError("ConnectCommandHandler"));
        }
    }
}