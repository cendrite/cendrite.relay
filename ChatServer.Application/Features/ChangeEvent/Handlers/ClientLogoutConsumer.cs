using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Contracts.Repositories;
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
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
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

            foreach (var connectionId in connectionIds)
            {
                var client = await clientCache.GetAsync(connectionId);

                if (client?.GetTransientId(HubType.ChangeEvent) is not null and var id)
                {
                    await notifier.NotifyAsync(id, new Base.ChangeEvent(
                                Guid.NewGuid(),
                                ChangeEventType.UserLoggedOut,
                                "UserLogout",
                                "User logged out",
                                new UserLoggedOutEvent
                                {
                                    ConnectionId = connectionId
                                }));

                }
                else
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                }
            }

            await clientRepository.DeleteAsync(context.Message.DeviceId, context.CancellationToken);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming UserLogoutMessage: {Message}", context.Message);
            throw;
        }
    }
}
