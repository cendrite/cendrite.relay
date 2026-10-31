using Cendrite.Persistence.Outbox;

namespace ChatServer.Infrastructure.Persistence.Repositories;

internal interface IOutboxRepository
{
    Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxMessage message);
}
