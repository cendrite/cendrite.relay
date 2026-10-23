using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Payloads;
using ChatServer.Application.Features.ChangeEvent.Services;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Friendship;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class FriendDeletedConsumer(
    ChangeEventDispatcher dispatcher,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
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

                if (client is null)
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

                await dispatcher.DispatchAsync(client, changeEvent, context.CancellationToken);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consuming FriendshipDeletedMessage: {Message}", context.Message);
            throw;
        }
    }
}
