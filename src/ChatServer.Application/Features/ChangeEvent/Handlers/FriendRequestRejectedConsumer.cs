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

public class FriendRequestRejectedConsumer(
    IChangeEventNotifier notifier,
    IClientRepository clientRepository,
    ILogger<FriendRequestRejectedConsumer> logger)
    : IConsumer<FriendRequestRejectedMessage>
{
    public async Task Consume(ConsumeContext<FriendRequestRejectedMessage> context)
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

                var friendRequest = new FriendRequestRejectedEvent
                {
                    Tag = message.RejectedUserProfileTag
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRequestRejected,
                    "FriendRequest",
                    "Friend request rejected",
                    friendRequest);

                await notifier.NotifyAsync(connection.TransientId, changeEvent, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendRequestRejectedMessage: {Message}", context.Message);
            throw;
        }
    }
}
