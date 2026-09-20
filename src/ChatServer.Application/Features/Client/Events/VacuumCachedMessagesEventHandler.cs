using System.Runtime.CompilerServices;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.Messaging.Models;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;

public sealed class VacuumCachedMessagesEventHandler(
    IMessageRepository messageRepository,
    IMessageSender messageSender,
    IUnitOfWork uow)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public async Task Handle(ClientConnectedEvent notification, CancellationToken cancellationToken)
    {
        await foreach (var messages in GetBatchCachedMessagesAsync(notification.ClientId, 10, cancellationToken))
        {
            foreach (var message in messages)
            {
                var outgoingMessage = OutgoingMessage.MapFrom(message);
                await messageSender.SendMessageAsync(notification.TransientId, outgoingMessage, cancellationToken);
            }

            messageRepository.DeleteRange(messages);
            await uow.SaveChangesAsync(cancellationToken);
        }
    }

    public async IAsyncEnumerable<IReadOnlyCollection<ChatMessage>> GetBatchCachedMessagesAsync(
        Guid recipientId,
        int batchCount,
        [EnumeratorCancellation] CancellationToken ct)
    {
        while (true)
        {
            var messages = await messageRepository.GetBatchMessagesAsync(recipientId, batchCount, ct);

            if (messages.Count == 0)
                yield break;

            yield return messages;
        }
    }
}
