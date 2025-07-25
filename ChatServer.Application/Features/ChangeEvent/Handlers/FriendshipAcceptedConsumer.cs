using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendshipAcceptedConsumer(
    IClientCache clientCache,
    IChangeEventNotifier notifier,
    ILogger<FriendshipAcceptedConsumer> logger)
    : IConsumer<FriendshipAcceptedMessage>
{
    public async Task Consume(ConsumeContext<FriendshipAcceptedMessage> context)
    {
        try
        {
            var message = context.Message;

            foreach (var connection in message.RouteIds)
            {
                var client = await clientCache.GetAsync(connection);
                if (client?.GetTransientId(HubType.ChangeEvent) is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connection);
                    continue;
                }

                var friendAccepted = new FriendRequestAcceptedEvent()
                {
                    Username = message.Username,
                    Tag = message.Tag,
                    DisplayName = message.DisplayName,
                    ProfilePictureUrl = message.ProfilePictureUrl,
                    ConnectionIds = message.ConnectionIds
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRequestAccepted,
                    "FriendshipAccepted",
                    "Friendship accepted",
                    friendAccepted);

                await notifier.NotifyAsync(client.TransientIds[HubType.ChangeEvent], changeEvent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipAcceptedMessage: {Message}", context.Message);
            throw;
        }
    }
}