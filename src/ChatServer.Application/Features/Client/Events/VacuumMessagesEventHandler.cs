using System.Runtime.CompilerServices;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Messaging.Models;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;

public sealed class VacuumCachedMessagesEventHandler(
    IClientRepository clientRepository,
    IMessageRepository messageRepository,
    IMessageSender messageSender,
    IUnitOfWork uow)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public async Task Handle(ClientConnectedEvent ntf, CancellationToken ct)
    {
        var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(ntf.ConnectionId), ct);

        if (client?.GetConnection(HubType.Message.ToString())?.TransientId is not { } transientId)
            return;

        await foreach (var messages in GetBatchCachedMessagesAsync(ntf.ConnectionId, 10, ct))
        {
            foreach (var message in messages)
            {
                var outgoingMessage = OutgoingMessage.MapFrom(message);
                await messageSender.SendMessageAsync(transientId, outgoingMessage, ct);
            }

            messageRepository.DeleteRange(messages);
            await uow.SaveChangesAsync(ct);
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
