using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cendrite.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendshipAcceptedConsumer(
    IClientRepository clientRepository,
    ChangeEventDispatcher dispatcher,
    IUnitOfWork unitOfWork,
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

                if (client is null)
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

                await dispatcher.DispatchAsync(client, changeEvent, context.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipAcceptedMessage: {Message}", context.Message);
            throw;
        }
    }
}
