using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using Cendrite.Message.Contracts.Messages.User;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public sealed class AccountKeyRotatedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<AccountKeyRotatedConsumer> logger)
    : IConsumer<AccountKeyRotatedMessage>
{
    public async Task Consume(ConsumeContext<AccountKeyRotatedMessage> ctx)
    {
        try
        {
            var clients = await clientRepository.GetActiveByUserIdAsync(ctx.Message.UserId, ctx.CancellationToken);

            foreach (var client in clients.Where(c => c.ConnectionId != ctx.Message.ConnectionId))
            {
                await dispatcher.DispatchAsync(client, new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.AccountKeyRotated,
                    "AccountKeyRotated",
                    "Recovery phrase changed",
                    new AccountKeyRotatedEvent
                    {
                        DeviceId = ctx.Message.DeviceId,
                        ConnectionId = ctx.Message.ConnectionId,
                        DeviceName = ctx.Message.DeviceName,
                    }), ctx.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(ctx.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming AccountKeyRotatedMessage: {Message}", ctx.Message);
            throw;
        }
    }
}
