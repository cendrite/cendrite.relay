using Cypherly.ChatServer.Application.Abstractions;
using Cypherly.ChatServer.Domain.Events.Client;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Client;
using Microsoft.Extensions.Logging;

namespace Cypherly.ChatServer.Application.Features.Client.Events;

public class ClientConnectedEventHandler(
    IProducer<ClientConnectedMessage> producer,
    ILogger<ClientConnectedEventHandler> logger)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public async Task Handle(ClientConnectedEvent notification, CancellationToken cancellationToken)
    {
        var message = new ClientConnectedMessage()
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = notification.ClientId
        };
        
        await producer.PublishMessageAsync(message, cancellationToken);

        logger.LogInformation("Clients connected: {ClientId}", notification.ClientId);
    }
}