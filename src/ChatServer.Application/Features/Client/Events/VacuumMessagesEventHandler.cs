using System.Runtime.CompilerServices;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Messaging.Models;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;

namespace ChatServer.Application.Features.Client.Events;

public sealed class VacuumMessagesEventHandler(
    IClientRepository clientRepository,
    IMessageRepository messageRepository,
    IMessageSender messageSender,
    IUnitOfWork uow)
    : IDomainEventHandler<VacuumMessagesEvent>
{
    private const int BatchSize = 10;

    public async Task Handle(VacuumMessagesEvent ntf, CancellationToken ct)
    {
        var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(ntf.ConnectionId), ct);

        if (client?.GetConnection(HubType.Message.ToString())?.TransientId is not { } transientId)
            return;

        await foreach (var messages in GetBatchMessagesAsync(ntf.ConnectionId, BatchSize, ct))
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

    private async IAsyncEnumerable<IReadOnlyCollection<Message>> GetBatchMessagesAsync(
        Guid recipientId,
        int batchCount,
        [EnumeratorCancellation] CancellationToken ct)
    {
        MessageCursor? cursor = null;

        while (true)
        {
            var messages = await messageRepository.GetBatchMessagesAsync(recipientId, batchCount, cursor, ct);

            if (messages.Count == 0)
                yield break;

            yield return messages;

            if (messages.Count < batchCount)
                yield break;

            var last = messages.Last();
            cursor = new MessageCursor(last.Timestamp, last.Id);
        }
    }
}
