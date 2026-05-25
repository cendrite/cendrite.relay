using ChatServer.Infrastructure.Persistence.Context;
using Cypherly.Persistence.Outbox;
using Cypherly.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Repositories;

public class OutboxRepository(ChatServerDbContext context) : IOutboxRepository
{
    /// <summary>
    /// Get unprocessed outbox messages
    /// </summary>
    /// <param name="batchSize">Amount of outbox messages retrieved</param>
    /// <returns>An Array of <see cref="OutboxMessage"/></returns>
    public async Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize)
    {
        return await context.OutboxMessage
            .Where(m => m.ProcessedOn == null)
            .Take(batchSize)
            .ToArrayAsync();
    }

    /// <summary>
    /// Marks an outbox message as processed by setting the ProcessedOn property to the current date and time.
    /// </summary>
    /// <param name="message">The <see cref="OutboxMessage"/> that will be marked as processed</param>
    /// <returns></returns>
    public Task MarkAsProcessedAsync(OutboxMessage message)
    {
        message.ProcessedOn = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}