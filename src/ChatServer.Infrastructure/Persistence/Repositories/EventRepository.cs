using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Repositories;

internal sealed class EventRepository(ChatServerDbContext ctx) : IEventRepository
{
    public async Task CreateAsync(Event entity, CancellationToken ct = default)
    {
        await ctx.Event.AddAsync(entity, ct);
    }

    public void Delete(Event entity)
    {
        ctx.Event.Remove(entity);
    }

    public void DeleteRange(IEnumerable<Event> events)
    {
        ctx.Event.RemoveRange(events);
    }

    public async Task<Event?> GetAsync(ISpecification<Event> spec, CancellationToken ct = default)
    {
        var q = ctx.Event.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<Event>> GetBatchEventsAsync(
        Guid recipientId,
        int batchCount,
        PageCursor? after = null,
        CancellationToken ct = default)
    {
        var query = ctx.Event.Where(x => x.RecipientId == recipientId);

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

    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ctx.Event.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException($"Event with id {id} not found.");
    }

    public void Update(Event entity, CancellationToken ct = default)
    {
        ctx.Event.Update(entity);
    }
}
