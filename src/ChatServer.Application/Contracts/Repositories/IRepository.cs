using ChatServer.Application.Abstractions;
using ChatServer.Domain.Abstractions;

namespace ChatServer.Application.Contracts.Repositories;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task CreateAsync(T entity, CancellationToken ct = default);
    void Delete(T entity);
    Task<T> GetByIdAsync(Guid id, CancellationToken ct = default);
}
