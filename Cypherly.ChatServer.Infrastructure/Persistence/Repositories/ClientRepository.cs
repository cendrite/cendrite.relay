using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Contracts.Repositories;
using Cypherly.ChatServer.Domain.Aggregates;
using Cypherly.ChatServer.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Cypherly.ChatServer.Infrastructure.Persistence.Repositories;

public class ClientRepository(ChatServerDbContext context) : IClientRepository
{
    public async Task CreateAsync(Client entity)
    {
        await context.Client.AddAsync(entity);
    }

    public Task DeleteAsync(Client entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await context.Client.FindAsync(id);
    }

    public Task UpdateAsync(Client entity)
    {
        throw new NotImplementedException();
    }

    public Task QueryAsync(Guid id, Func<IQueryable<Client>, IQueryable<Client>>? includeFunc)
    {
        IQueryable<Client> query = context.Set<Client>();

        if (includeFunc is not null)
        {
            query = includeFunc(query);
        }

        return query.FirstOrDefaultAsync(c => c.Id == id);
    }
}