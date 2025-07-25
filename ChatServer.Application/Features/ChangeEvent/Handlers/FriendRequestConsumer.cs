using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendRequestConsumer(
    IChangeEventNotifier notifier,
    IClientCache clientCache,
    ILogger<FriendRequestConsumer> logger)
    : IConsumer<FriendRequestMessage>
{
    public async Task Consume(ConsumeContext<FriendRequestMessage> context)
    {
        try
        {
            var message = context.Message;

            foreach (var connection in message.ConnectionIds)
            {
                var client = await clientCache.GetAsync(connection);
                if (client?.GetTransientId(HubType.ChangeEvent) is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connection);
                    continue;
                }

                var friendRequest = new FriendRequestEvent
                {
                    Username = message.FriendUsername,
                    Tag = message.FriendTag,
                    RequestedAt = message.FriendRequestDate,
                    IsSeen = message.IsSeen,
                    DisplayName = message.FriendDisplayName,
                    PresignedProfilePictureUrl = message.FriendProfilePictureUrl
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRequestSent,
                    "FriendRequest",
                    "Received a friend request",
                    friendRequest);

                await notifier.NotifyAsync(client.TransientIds[HubType.ChangeEvent], changeEvent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendRequestMessage: {Message}", context.Message);
            throw;
        }
    }
}