using ChatServer.Domain.Abstractions;

namespace ChatServer.Application.Contracts.Repositories;

public interface IRepository<T> where T : AggregateRoot
{
    Task CreateAsync(T entity, CancellationToken ct = default);
    void Delete(T entity);
    Task<T> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
}
