using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;

namespace ChatServer.Application.Features.ChangeEvent.Services;

/// <summary>
/// Delivers a change event to a client's change event hub, or holds it on the client until it connects.
/// Held events are persisted by the caller's unit of work and flushed on connect.
/// </summary>
public sealed class ChangeEventDispatcher(IChangeEventNotifier notifier)
{
    public async Task DispatchAsync(Domain.Aggregates.Client client, Base.ChangeEvent changeEvent, CancellationToken ct = default)
    {
        if (client.GetConnection(HubType.ChangeEvent.ToString()) is { } connection)
        {
            await notifier.NotifyAsync(connection.TransientId, changeEvent, ct);
            return;
        }

        client.AddEvent(changeEvent.ToEvent(client.ConnectionId));
    }
}
