using Cypherly.ChatServer.Domain.Aggregates;

namespace Cypherly.ChatServer.Application.Contracts.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task QueryAsync(Guid id, Func<IQueryable<Client>, IQueryable<Client>>? includeFunc);
}