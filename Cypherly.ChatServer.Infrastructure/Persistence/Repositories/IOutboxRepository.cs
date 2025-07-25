using Cypherly.Persistence.Outbox;

namespace Cypherly.Persistence.Repositories;

public interface IOutboxRepository
{
    Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxMessage message);
}