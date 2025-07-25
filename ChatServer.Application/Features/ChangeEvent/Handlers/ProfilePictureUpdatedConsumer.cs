using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Profile;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class ProfilePictureUpdatedConsumer(
    IChangeEventNotifier changeEventNotifier,
    IClientCache clientCache,
    ILogger<ProfilePictureUpdatedConsumer> logger)
    : IConsumer<ProfilePictureUpdatedMessage>
{
    public async Task Consume(ConsumeContext<ProfilePictureUpdatedMessage> context)
    {
        try
        {
            logger.LogInformation("received ProfilePictureUpdatedMessage: {@Message}", context.Message);
            var message = context.Message;
            var eventData = new ProfilePictureUpdatedChangeEvent(message.UserTag, message.ProfilePictureUrl);
            var changeEvent = new Base.ChangeEvent(Guid.NewGuid(), ChangeEventType.ProfilePictureChanged, "UserProfile", "Profile picture updated", eventData);

            foreach (var connectionId in message.ConnectionIds)
            {
                var client = await clientCache.GetAsync(connectionId, context.CancellationToken);
                if (client?.GetTransientId(HubType.ChangeEvent) is null)
                {
                    logger.LogWarning("Clients not found for connectionId {ConnectionId}", connectionId);
                    continue;
                }

                await changeEventNotifier.NotifyAsync(client.TransientIds[HubType.ChangeEvent], changeEvent, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ProfilePictureUpdatedMessage");
            throw;
        }
    }
}