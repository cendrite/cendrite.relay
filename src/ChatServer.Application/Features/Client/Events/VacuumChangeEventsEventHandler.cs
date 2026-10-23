using System.Runtime.CompilerServices;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;

namespace ChatServer.Application.Features.Client.Events;

public sealed class VacuumChangeEventsEventHandler(
    IClientRepository clientRepository,
    IEventRepository eventRepository,
    IChangeEventNotifier notifier,
    IUnitOfWork uow)
    : IDomainEventHandler<VacuumChangeEventsEvent>
{
    private const int BatchSize = 10;

    public async ValueTask Handle(VacuumChangeEventsEvent ntf, CancellationToken ct)
    {
        var client = await clientRepository.GetAsync(new ClientWithConnectionSpec(ntf.ConnectionId), ct);

        if (client?.GetConnection(HubType.ChangeEvent.ToString())?.TransientId is not { } transientId)
            return;

        await foreach (var events in GetBatchEventsAsync(ntf.ConnectionId, BatchSize, ct))
        {
            foreach (var evt in events)
            {
                var changeEvent = ChangeEvent.Base.ChangeEvent.FromEvent(evt);
                await notifier.NotifyAsync(transientId, changeEvent, ct);
            }

            eventRepository.DeleteRange(events);
            await uow.SaveChangesAsync(ct);
        }
    }

    private async IAsyncEnumerable<IReadOnlyCollection<Event>> GetBatchEventsAsync(
        Guid recipientId,
        int batchCount,
        [EnumeratorCancellation] CancellationToken ct)
    {
        PageCursor? cursor = null;

        while (true)
        {
            var events = await eventRepository.GetBatchEventsAsync(recipientId, batchCount, cursor, ct);

            if (events.Count == 0)
                yield break;

            yield return events;

            if (events.Count < batchCount)
                yield break;

            var last = events.Last();
            cursor = new PageCursor(last.Created, last.Id);
        }
    }
}
