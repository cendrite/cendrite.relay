using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Aggregates;
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Repositories;

internal sealed class ClientRepository(ChatServerDbContext ctx) : IClientRepository
{
    public async Task<Client?> GetAsync(ISpecification<Client> spec, CancellationToken ct = default)
    {
        var q = ctx.Client.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public async Task CreateAsync(Client entity, CancellationToken ct = default)
    {
        await ctx.Client.AddAsync(entity, ct);
    }

    public async Task<Client> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await ctx.Client.FindAsync([id], cancellationToken: ct)
            ?? throw new InvalidOperationException($"Client with ID {id} not found.");
    }

    public Task QueryAsync(Guid id, Func<IQueryable<Client>, IQueryable<Client>>? includeFunc, CancellationToken ct = default)
    {
        IQueryable<Client> query = ctx.Set<Client>();

        if (includeFunc is not null)
        {
            query = includeFunc(query);
        }

        return query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }

    public async Task<IReadOnlyCollection<Client>> GetWithExpiredMediaAsync(
        DateTime cutoff,
        int batchCount,
        CancellationToken ct = default)
    {
        return await ctx.Client
            .Where(c => c.Media.Any(m => m.Created < cutoff))
            .Include(c => c.Media.Where(m => m.Created < cutoff))
            .Take(batchCount)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Client>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await ctx.Client
            .Where(c => c.UserId == userId && c.Deleted == null)
            .Include(c => c.Connections)
            .ToListAsync(ct);
    }

    public void Delete(Client entity)
    {
        ctx.Client.Remove(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await ctx.Client.FindAsync([id], cancellationToken: ct)
            ?? throw new InvalidOperationException($"Client with ID {id} not found.");
        ctx.Client.Remove(entity);
    }
}
