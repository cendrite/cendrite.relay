using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Messages.User;
using Cypherly.Message.Contracts.Responses.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class ClientLogoutConsumer(
    ChangeEventDispatcher dispatcher,
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
            var proxyResult = await GetConnectionIds(context.Message);

            foreach (var connectionId in proxyResult.Item2)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), context.CancellationToken);

                if (client is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                    continue;
                }

                await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.UserLoggedOut,
                    "UserLogout",
                    "User logged out",
                    new UserLoggedOutEvent
                    {
                        UserTag = proxyResult.Item1,
                        ConnectionId = context.Message.ConnectionId,
                    }), context.CancellationToken);
            }

            var loggedOutClient = await clientRepository.GetAsync(new ClientByIdSpec(context.Message.DeviceId), context.CancellationToken);

            if (loggedOutClient is null)
                logger.LogWarning("No client found for device id: {DeviceId}", context.Message.DeviceId);
            else
                loggedOutClient.SetDelete();

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming UserLogoutMessage: {Message}", context.Message);
            throw;
        }
    }

    private async Task<(string, IReadOnlyCollection<Guid>)> GetConnectionIds(UserLogoutMessage message)
    {
        var response = await connectionIdsClient.GetResponse<ConnectionIdsProxyResponse>(new ConnectionIdsProxyMessage
        {
            CorrelationId = message.CorrelationId,
            CausationId = message.Id,
            TenantId = message.UserId
        });

        if (!response.Message.IsSuccess)
        {
            logger.LogError("Failed to retrieve connection IDs for user logout: Error: {Error}, DeviceID: {DeviceId}",
                response.Message.Error, message.DeviceId);
            throw new Exception("Failed to retrieve connection IDs for user logout");
        }

        return new ValueTuple<string, IReadOnlyCollection<Guid>>(response.Message.UserTag,
            response.Message.ConnectionIds?.SelectMany(kv => kv.Value).Distinct().ToList() ?? []);
    }
}
