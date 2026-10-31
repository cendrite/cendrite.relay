using ChatServer.Application.Abstractions;
using ChatServer.Domain.Events.Client;
using Cendrite.Message.Contracts.Abstractions;
using Cendrite.Message.Contracts.Messages.Client;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Events;

public class ClientConnectedEventHandler(
    IProducer<ClientConnectedMessage> producer,
    IMessageRepository messageRepository,
    ILogger<ClientConnectedEventHandler> logger)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public async ValueTask Handle(ClientConnectedEvent ntf, CancellationToken ct)
    {
        var message = new ClientConnectedMessage()
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = ntf.ClientId
        };
        
        await producer.PublishMessageAsync(message, ct);

        logger.LogInformation("Clients connected: {ClientId}", ntf.ClientId);
    }
}
