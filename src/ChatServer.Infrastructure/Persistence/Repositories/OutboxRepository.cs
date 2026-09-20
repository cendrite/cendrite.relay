using ChatServer.Infrastructure.Persistence.Context;
using Cypherly.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Repositories;

internal sealed class OutboxRepository(ChatServerDbContext ctx) : IOutboxRepository
{
    /// <summary>
    /// Get unprocessed outbox messages
    /// </summary>
    /// <param name="batchSize">Amount of outbox messages retrieved</param>
    /// <returns>An Array of <see cref="OutboxMessage"/></returns>
    public async Task<OutboxMessage[]> GetUnprocessedAsync(int batchSize)
    {
        return await ctx.OutboxMessage
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
