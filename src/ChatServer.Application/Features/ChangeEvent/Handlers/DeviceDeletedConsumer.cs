using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cendrite.Message.Contracts.Messages.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class DeviceDeletedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceDeletedConsumer> logger)
    : IConsumer<DeviceDeletedMessage>
{
    public async Task Consume(ConsumeContext<DeviceDeletedMessage> ctx)
    {
        try
        {
            var client = await clientRepository.GetAsync(new ClientWithConnectionsByIdSpec(ctx.Message.DeviceId), ctx.CancellationToken);
            if (client is null)
            {
                logger.LogWarning("No client found for device id: {DeviceId}", ctx.Message.DeviceId);
                return;
            }

            await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                Guid.NewGuid(),
                ChangeEventType.Deleted,
                "ClientDeleted",
                "Client deleted",
                new ClientDeletedEvent(client.ConnectionId)), ctx.CancellationToken);

            await unitOfWork.SaveChangesAsync(ctx.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming DeviceDeletedMessage: {Message}", ctx.Message);
            throw;
        }
    }
}
