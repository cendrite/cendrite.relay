using System.Runtime.CompilerServices;
using ChatServer.Application.Abstractions;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;

public sealed class VacuumCachedMessagesEventHandler(
    IMessageRepository messageRepository)
    : IDomainEventHandler<ClientConnectedEvent>
{
    public Task Handle(ClientConnectedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<IReadOnlyList<ChatMessage>> GetBatchCachedMessagesAsync(
        int batchCount, 
        Func<int, CancellationToken> query,
        [EnumeratorCancellation] CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
