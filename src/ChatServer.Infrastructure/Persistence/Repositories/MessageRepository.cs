using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

internal sealed class MessageRepository(ChatServerDbContext ctx) : IMessageRepository
{
    public async Task CreateAsync(Message entity, CancellationToken ct = default)
    {
        await ctx.ChatMessage.AddAsync(entity);
    }

    public void Delete(Message entity)
    {
        ctx.ChatMessage.Remove(entity);
    }

    public void DeleteRange(IEnumerable<Message> messages)
    {
        ctx.ChatMessage.RemoveRange(messages);
    }

    public async Task<Message?> GetAsync(ISpecification<Message> spec, CancellationToken ct = default)
    {
        var q = ctx.ChatMessage.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<Message>> GetBatchMessagesAsync(
        Guid recipientId,
        int batchCount,
        PageCursor? after = null,
        CancellationToken ct = default)
    {
        var query = ctx.ChatMessage.Where(x => x.RecipientId == recipientId);

        if (after is { } cursor)
        {
            var created = cursor.Created;
            var id = cursor.Id;

            query = query.Where(x => x.Created > created || (x.Created == created && x.Id > id));
        }

        return await query
            .OrderBy(x => x.Created)
            .ThenBy(x => x.Id)
            .Take(batchCount)
            .ToListAsync(ct);
    }

    public async Task<Message> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var message = await ctx.ChatMessage.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (message is null)
        {
            throw new InvalidOperationException($"Message with id {id} not found.");
        }

        return message;
    }

    public void Update(Message entity, CancellationToken ct = default)
    {
        ctx.ChatMessage.Update(entity);
    }
}
