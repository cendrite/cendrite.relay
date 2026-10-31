using Cendrite.Message.Contracts.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Infrastructure.Messaging;

public class Producer<TMessage>(
    IPublishEndpoint publishEndpoint,
    ILogger<Producer<TMessage>> logger)
    : IProducer<TMessage>
    where TMessage : IBaseMessage
{

    public async Task PublishMessageAsync(TMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await publishEndpoint.Publish(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {Messaging} for message with id: {Id}",
                nameof(Producer<TMessage>),
                message.Id);
            throw;
        }
    }
}