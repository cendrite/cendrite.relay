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

public class FriendshipAcceptedConsumer(
    IClientRepository clientRepository,
    IChangeEventNotifier notifier,
    ILogger<FriendshipAcceptedConsumer> logger)
    : IConsumer<FriendshipAcceptedMessage>
{
    public async Task Consume(ConsumeContext<FriendshipAcceptedMessage> context)
    {
        try
        {
            var message = context.Message;

            foreach (var connectionId in message.RouteIds)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), context.CancellationToken);
                var connection = client?.GetConnection(HubType.ChangeEvent.ToString());

                if (connection is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                    continue;
                }

                var friendAccepted = new FriendRequestAcceptedEvent()
                {
                    Username = message.Username,
                    Tag = message.Tag,
                    DisplayName = message.DisplayName,
                    AvatarKey = message.AvatarKey,
                    ConnectionIds = message.ConnectionIds
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRequestAccepted,
                    "FriendshipAccepted",
                    "Friendship accepted",
                    friendAccepted);

                await notifier.NotifyAsync(connection.TransientId, changeEvent, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipAcceptedMessage: {Message}", context.Message);
            throw;
        }
    }
}
