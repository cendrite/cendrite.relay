using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Responses.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class DeviceDeletedConsumer(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<ConnectionIdsProxyMessage> conne,
    ChangeEventDispatcher dispatcher,
    ILogger<DeviceDeletedConsumer> logger)
    : IConsumer<DeviceDeletedMessage>
{
    public async Task Consume(ConsumeContext<DeviceDeletedMessage> ctx)
    {
        try
        {
            var proxyResult = await GetConnectionIds(Guid.Empty, Guid.Empty, Guid.Empty);

            foreach (var conId in proxyResult.Item2)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(conId), ctx.CancellationToken);
                if (client is null)
                {
                    logger.LogWarning("No client found for connection id: {ConnectionId}", conId);
                    continue;
                }

                var chageEvent = new ClientDeletedEvent(ctx.Message.DeviceId);

                await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.Deleted,
                    "ClientDeleted",
                    "Client deleted",
                    chageEvent), ctx.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming DeviceDeletedMessage: {Message}", ctx.Message);
            throw;
        }
    }
    private async Task<(string, IReadOnlyCollection<Guid>)> GetConnectionIds(Guid userId, Guid causationId, Guid correlationId)
    {
        var response = await conne.GetResponse<ConnectionIdsProxyResponse>(new ConnectionIdsProxyMessage
        {
            CorrelationId = correlationId,
            CausationId = causationId,
            TenantId = userId
        });

        if (!response.Message.IsSuccess)
        {
            logger.LogError("Failed to retrieve connection IDs for user logout: Error: {Error}, DeviceID: {DeviceId}",
                response.Message.Error, userId);
            throw new Exception("Failed to retrieve connection IDs for user logout");
        }

        return new ValueTuple<string, IReadOnlyCollection<Guid>>(response.Message.UserTag,
            response.Message.ConnectionIds?.SelectMany(kv => kv.Value).Distinct().ToList() ?? []);
    }



}
