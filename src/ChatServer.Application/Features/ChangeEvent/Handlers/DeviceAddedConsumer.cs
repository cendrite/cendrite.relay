using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using Cendrite.Message.Contracts.Messages.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class DeviceAddedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceAddedConsumer> logger)
    : IConsumer<DeviceAddedMessage>
{
    public async Task Consume(ConsumeContext<DeviceAddedMessage> ctx)
    {
        try
        {
            var clients = await clientRepository.GetActiveByUserIdAsync(ctx.Message.UserId, ctx.CancellationToken);

            foreach (var client in clients.Where(c => c.ConnectionId != ctx.Message.ConnectionId))
            {
                await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.DeviceAdded,
                    "DeviceAdded",
                    "Device added",
                    new DeviceAddedEvent
                    {
                        DeviceId = ctx.Message.DeviceId,
                        ConnectionId = ctx.Message.ConnectionId,
                        DevicePlatform = ctx.Message.DevicePlatform,
                    }), ctx.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(ctx.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming DeviceAddedMessage: {Message}", ctx.Message);
            throw;
        }
    }
}
