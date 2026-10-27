using ChatServer.Domain.Aggregates;

namespace ChatServer.Application.Contracts.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task QueryAsync(Guid id, Func<IQueryable<Client>, IQueryable<Client>>? includeFunc, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyCollection<Client>> GetWithExpiredMediaAsync(DateTime cutoff, int batchCount, CancellationToken ct = default);

    Task<IReadOnlyCollection<Client>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
}
