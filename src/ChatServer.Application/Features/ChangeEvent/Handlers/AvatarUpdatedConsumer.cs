using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Profile;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class AvatarUpdatedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<AvatarUpdatedConsumer> logger)
    : IConsumer<AvatarUpdatedMessage>
{
    public async Task Consume(ConsumeContext<AvatarUpdatedMessage> ctx)
    {
        try
        {
            var message = ctx.Message;
            var eventData = new AvatarUpdatedEvent(message.UserTag, message.AvatarKey);
            var changeEvent = new Base.ChangeEvent(Guid.NewGuid(), ChangeEventType.AvatarChanged, "UserProfile", "Profile picture updated", eventData);

            foreach (var connectionId in message.ConnectionIds)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), ctx.CancellationToken);

                if (client is null) continue;

                await dispatcher.DispatchAsync(client, changeEvent, ctx.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(ctx.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing AvatarUpdatedMessage: {@Message}", ctx.Message);
            throw;
        }
    }
}
