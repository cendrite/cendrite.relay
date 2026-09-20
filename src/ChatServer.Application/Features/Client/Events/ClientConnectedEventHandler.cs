using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Domain.Events.Client;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Client;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Client.Events;

public class ClientConnectedEventHandler(
    IProducer<ClientConnectedMessage> producer,
    IMessageCache messageCache,
    IMessageRepository messageRepository,
    ILogger<ClientConnectedEventHandler> logger)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public async Task Handle(ClientConnectedEvent ntf, CancellationToken ct)
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
