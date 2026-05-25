namespace ChatServer.Application.Contracts.Cache;

public interface ICache<T>
{
    Task AddAsync(T value, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}