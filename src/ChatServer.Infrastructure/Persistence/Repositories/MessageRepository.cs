using ChatServer.Application.Abstractions;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

internal sealed class MessageRepository(ChatServerDbContext ctx) : IMessageRepository
{
    public async Task CreateAsync(ChatMessage entity, CancellationToken ct = default)
    {
        await ctx.ChatMessage.AddAsync(entity);
    }

    public void Delete(ChatMessage entity)
    {
        ctx.ChatMessage.Remove(entity);
    }

    public void DeleteRange(IEnumerable<ChatMessage> messages)
    {
        ctx.ChatMessage.RemoveRange(messages);
    }

    public async Task<ChatMessage?> GetAsync(ISpecification<ChatMessage> spec, CancellationToken ct = default)
    {
        var q = ctx.ChatMessage.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<ChatMessage>> GetBatchMessagesAsync(Guid recipientId, int batchCount, CancellationToken ct = default)
    {
        return await ctx.ChatMessage
            .Where(x => x.RecipientId == recipientId)
            .OrderByDescending(x => x.Timestamp)
            .Take(batchCount)
            .ToListAsync(ct);
    }

    public async Task<ChatMessage> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var message = await ctx.ChatMessage.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (message is null)
        {
            throw new InvalidOperationException($"Message with id {id} not found.");
        }

        return message;
    }

    public void Update(ChatMessage entity, CancellationToken ct = default)
    {
        ctx.ChatMessage.Update(entity);
    }
}
