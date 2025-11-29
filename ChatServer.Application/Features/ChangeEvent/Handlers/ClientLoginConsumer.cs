using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Messages.User;
using Cypherly.Message.Contracts.Responses.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers
{
    public sealed class ClientLoginConsumer(
        IChangeEventNotifier notifier,
        IClientCache clientCache,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        IRequestClient<ConnectionIdsProxyMessage> connectionIdsClient,
        ILogger<ClientLoginConsumer> logger)
        : IConsumer<UserLoginMessage>
    {
        public async Task Consume(ConsumeContext<UserLoginMessage> context)
        {
            try
            {
                var proxyResult = await GetConnectionIds(context.Message);

                foreach (var connectionId in proxyResult.Item2)
                {
                    var client = await clientCache.GetAsync(connectionId);

                    if (client?.GetTransientId(HubType.ChangeEvent) is not null and var id)
                    {
                        await notifier.NotifyAsync(id, new Base.ChangeEvent(
                            Guid.NewGuid(),
                            ChangeEventType.UserLoggedIn,
                            "UserLogin",
                            "User logged in",
                            new UserLoggedInEvent
                            {
                                UserTag = proxyResult.Item1,
                                ConnectionId = context.Message.ConnectionId,
                            }));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming UserLoginMessage: {Message}", context.Message);
                throw;
            }
        }

        private async Task<(string, IReadOnlyCollection<Guid>)> GetConnectionIds(UserLoginMessage message)
        {
            var response = await connectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(
                new ConnectionIdsProxyMessage
                {
                    CorrelationId = message.CorrelationId,
                    CausationId = message.Id,
                    TenantId = message.UserId
                });

            if (!response.Message.IsSuccess)
            {
                logger.LogError(
                    "Failed to retrieve connection IDs for user logout: Error: {Error}, DeviceID: {DeviceId}",
                    response.Message.Error, message.DeviceId);
                throw new Exception("Failed to retrieve connection IDs for user logout");
            }

            return new ValueTuple<string, IReadOnlyCollection<Guid>>(response.Message.UserTag,
                response.Message.ConnectionIds?.SelectMany(kv => kv.Value).Distinct().ToList() ?? []);
        }
    }
}
