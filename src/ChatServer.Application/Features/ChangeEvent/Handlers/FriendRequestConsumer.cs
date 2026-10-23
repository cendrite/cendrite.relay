using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendRequestConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ILogger<FriendRequestConsumer> logger)
    : IConsumer<FriendRequestMessage>
{
    public async Task Consume(ConsumeContext<FriendRequestMessage> context)
    {
        try
        {
            var message = context.Message;

            foreach (var connectionId in message.ConnectionIds)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), context.CancellationToken);

                if (client is null)
                {
                    logger.LogWarning("Clients not found for connection ID: {ConnectionId}", connectionId);
                    continue;
                }

                var friendRequest = new FriendRequestEvent
                {
                    Username = message.FriendUsername,
                    Tag = message.FriendTag,
                    RequestedAt = message.FriendRequestDate,
                    IsSeen = message.IsSeen,
                    DisplayName = message.FriendDisplayName,
                    AvatarKey = message.AvatarKey
                };

                var changeEvent = new Base.ChangeEvent(
                    Guid.NewGuid(),
                    ChangeEventType.FriendRequestSent,
                    "FriendRequest",
                    "Received a friend request",
                    friendRequest);

                await dispatcher.DispatchAsync(client, changeEvent, context.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendRequestMessage: {Message}", context.Message);
            throw;
        }
    }
}
