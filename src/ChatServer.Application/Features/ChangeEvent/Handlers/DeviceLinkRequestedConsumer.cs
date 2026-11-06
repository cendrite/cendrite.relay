using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cendrite.Message.Contracts.Messages.Device;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class DeviceLinkRequestedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceLinkRequestedConsumer> logger)
    : IConsumer<DeviceLinkRequestedMessage>
{
    public async Task Consume(ConsumeContext<DeviceLinkRequestedMessage> ctx)
    {
        try
        {
            var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(ctx.Message.IssuingConnectionId), ctx.CancellationToken);
            if (client is null || client.UserId != ctx.Message.UserId)
            {
                logger.LogWarning("No client found for issuing connection {ConnectionId}", ctx.Message.IssuingConnectionId);
                return;
            }

            await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                Guid.NewGuid(),
                ChangeEventType.DeviceLinkRequested,
                "DeviceLinkRequested",
                "Device link requested",
                new DeviceLinkRequestedEvent
                {
                    PendingLinkId = ctx.Message.PendingLinkId,
                    DevicePlatform = ctx.Message.DevicePlatform,
                }), ctx.CancellationToken);

            await unitOfWork.SaveChangesAsync(ctx.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming DeviceLinkRequestedMessage: {Message}", ctx.Message);
            throw;
        }
    }
}
