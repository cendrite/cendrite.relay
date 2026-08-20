using ChatServer.Application.Cache.Client;
using ChatServer.Application.Enums;

namespace ChatServer.Application.Contracts.Cache;

public interface IClientCache
{
    Task<ClientCacheDto?> GetAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task<ClientCacheDto?> GetByTransientIdAsync(string transientId, CancellationToken cancellationToken);
    Task<bool> TryAddTransientIdAsync(Guid connectionId, HubType hubType, string transientId, CancellationToken cancellationToken);
    Task RemoveAsync(Guid connectionId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid connectionId, HubType hubType, string transientId, CancellationToken cancellationToken);
}