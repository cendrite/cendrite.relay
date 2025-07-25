using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Contracts.Cache;
using Cypherly.ChatServer.Application.Enums;
using Cypherly.ChatServer.Application.Features.ChangeEvent.Base;
using Cypherly.ChatServer.Application.Features.ChangeEvent.Event;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Cypherly.ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendDeletedConsumer(
    IChangeEventNotifier notifier,
    IClientCache clientCache,
    ILogger<FriendDeletedConsumer> logger)
    : IConsumer<FriendshipDeletedMessage>
{
    public async Task Consume(ConsumeContext<FriendshipDeletedMessage> context)
    {
        try
        {
            var message = context.Message;

            foreach (var connectionId in message.ConnectionIds)
            {
                var client = await clientCache.GetAsync(connectionId);
                if (client?.GetTransientId(HubType.ChangeEvent) is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                    continue;
                }

                var friendDeletedEvent = new FriendDeletedEvent
                {
                    Tag = message.DeletedUserTag,
                    DeletedBy = message.UserTag
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRemoved,
                    "FriendDeleted",
                    "Friend deleted",
                    friendDeletedEvent);

                await notifier.NotifyAsync(client.TransientIds[HubType.ChangeEvent], changeEvent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipDeletedMessage: {Message}", context.Message);
            throw;
        }
    }
}