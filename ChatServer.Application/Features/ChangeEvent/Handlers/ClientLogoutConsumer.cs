using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Messages.User;
using Cypherly.Message.Contracts.Responses.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class ClientLogoutConsumer(
        IChangeEventNotifier notifier,
        IClientCache clientCache,
        IRequestClient<ConnectionIdsProxyMessage> connectionIdsClient,
        ILogger<ClientLogoutConsumer> logger)
        : IConsumer<UserLogoutMessage>
{
    public async Task Consume(ConsumeContext<UserLogoutMessage> context)
    {
        try
        {

            var response = await connectionIdsClient.GetResponse<ConnectionIdsResponse>(new ConnectionIdsProxyMessage
            {
                CorrelationId = context.Message.CorrelationId,
                CausationId = context.Message.Id,
                TenantId = context.Message.UserId
            });

            if (response.Message.IsSuccess is false)
            {
                logger.LogError("Failed to retrieve connection IDs for user logout: Error: {Error}, DeviceID: {DeviceId}", response.Message.Error, context.Message.DeviceId);
                throw new Exception("Failed to retrieve connection IDs for user logout");
            }

            var connectionIds = response.Message.ConnectionIds?.SelectMany(kv => kv.Value).Distinct().ToList() ?? [];
            foreach(var connectionId in connectionIds)
            {
                var client = await clientCache.GetAsync(connectionId);
                if (client?.GetTransientId(HubType.ChangeEvent) is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                    continue;
                }

                await notifier.NotifyAsync(client.TransientIds[HubType.ChangeEvent], new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.UserLoggedOut,
                    "UserLogout",
                    "User logged out",
                    new UserLoggedOutEvent 
                    {
                        ConnectionId = connectionId
                    })); 
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming UserLogoutMessage: {Message}", context.Message);
            throw;
        }
    }
}
