using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Abstractions;
using ChatServer.Infrastructure.Persistence.Repositories;
using Cypherly.Persistence.Outbox;
using Mediator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace ChatServer.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessageJob(
    IOutboxRepository outboxRepository,
    IPublisher publisher,
    ILogger<ProcessOutboxMessageJob> logger,
    IUnitOfWork unitOfWork)
    : IJob
{
    /// <summary>
    /// Processes outbox messages by deserializing them and publishing them to a relevant handler through the mediator.
    /// </summary>
    /// <param name="context"></param>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var messages = await outboxRepository.GetUnprocessedAsync(20);
            if (messages.Length is 0) return;

            foreach (var message in messages)
            {
                var domainEvent = DeserializeDomainEvent(message);
                if (domainEvent == null) continue;

                await publisher.Publish(domainEvent);
                await outboxRepository.MarkAsProcessedAsync(message);
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing outbox message");
        }
    }

    /// <summary>
    /// Deserializes an outbox message into a domain event.
    /// </summary>
    /// <param name="outboxMessage">OutboxMessage to be deserialized</param>
    /// <returns>IDomainEvent Representing the deserialized <see cref="OutboxMessage"/></returns>
    private IDomainEvent? DeserializeDomainEvent(OutboxMessage outboxMessage)
    {
        var eventType = Type.GetType(outboxMessage.Type);

        if (eventType == null)
        {
            logger.LogError("Error getting type for message with id: {Id}", outboxMessage.Id);
            return null;
        }

        var domainEvent = (IDomainEvent?)JsonConvert.DeserializeObject(outboxMessage.Content, eventType);

        if (domainEvent == null)
        {
            logger.LogError("Error deserializing message with id: {Id}", outboxMessage.Id);
            return null;
        }

        return domainEvent;
    }
}
