using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.ChangeEvent.Event;
using ChatServer.Application.Specifications.Client;
using Cypherly.Message.Contracts.Messages.Profile;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.ChangeEvent.Handlers;

public class AvatarUpdatedConsumer(
    IChangeEventNotifier changeEventNotifier,
    IClientRepository clientRepository,
    ILogger<AvatarUpdatedConsumer> logger)
    : IConsumer<AvatarUpdatedMessage>
{
    public async Task Consume(ConsumeContext<AvatarUpdatedMessage> context)
    {
        try
        {
            logger.LogInformation("received AvatarUpdatedMessage: {@Message}", context.Message);
            var message = context.Message;
            var eventData = new AvatarUpdatedEvent(message.UserTag, message.AvatarKey);
            var changeEvent = new Base.ChangeEvent(Guid.NewGuid(), ChangeEventType.AvatarChanged, "UserProfile", "Profile picture updated", eventData);

            foreach (var connectionId in message.ConnectionIds)
            {
                var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(connectionId), context.CancellationToken);
                var connection = client?.GetConnection(HubType.ChangeEvent.ToString());

                if (connection is null) continue;

                await changeEventNotifier.NotifyAsync(connection.TransientId, changeEvent, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing AvatarUpdatedMessage: {@Message}", context.Message);
            throw;
        }
    }
}
