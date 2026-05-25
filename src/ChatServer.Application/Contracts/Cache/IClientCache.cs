using ChatServer.Application.Cache.Client;

namespace ChatServer.Application.Contracts.Cache;

public interface IClientCache : ICache<ClientCacheDto>
{
    Task<ClientCacheDto?> GetByTransientIdAsync(string transientId, CancellationToken cancellationToken);
    Task UpdateAsync(ClientCacheDto cacheDto, CancellationToken cancellationToken);
}