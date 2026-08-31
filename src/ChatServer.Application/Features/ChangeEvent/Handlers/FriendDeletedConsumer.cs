using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendDeletedConsumer(
    IChangeEventNotifier notifier,
    IClientRepository clientRepository,
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
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), context.CancellationToken);
                var connection = client?.GetConnection(HubType.ChangeEvent.ToString());

                if (connection is null)
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

                await notifier.NotifyAsync(connection.TransientId, changeEvent, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipDeletedMessage: {Message}", context.Message);
            throw;
        }
    }
}
