using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendRequestRejectedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
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

                if (client is null)
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

                await dispatcher.DispatchAsync(client, changeEvent, context.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendRequestRejectedMessage: {Message}", context.Message);
            throw;
        }
    }
}
