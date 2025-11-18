using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Aggregates;
using ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Persistence.Repositories;

public class ClientRepository(ChatServerDbContext dbContext) : IClientRepository
{
    public async Task CreateAsync(Client entity, CancellationToken ct = default)
    {
        await dbContext.Client.AddAsync(entity, ct);
    }

    public async Task<Client> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Client.FindAsync([id], cancellationToken: ct) 
            ?? throw new InvalidOperationException($"Client with ID {id} not found.");
    }

    public Task UpdateAsync(Client entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task QueryAsync(Guid id, Func<IQueryable<Client>, IQueryable<Client>>? includeFunc, CancellationToken ct = default)
    {
        IQueryable<Client> query = dbContext.Set<Client>();

        if (includeFunc is not null)
        {
            query = includeFunc(query);
        }

        return query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }

    public void Delete(Client entity)
    {
        dbContext.Client.Remove(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await dbContext.Client.FindAsync([id], cancellationToken: ct) 
            ?? throw new InvalidOperationException($"Client with ID {id} not found.");
        dbContext.Client.Remove(entity);
    }
}
