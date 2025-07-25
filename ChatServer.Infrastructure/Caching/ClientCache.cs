using ChatServer.Application.Cache.Client;
using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;

namespace ChatServer.Infrastructure.Caching;

public class ClientCache(IValkeyCacheService valkeyCacheService) : IClientCache
{
    /// <summary>
    /// Add a Clients to the cache by Clients ConnectionId
    /// Adds the inverse mapping of ConnectionId to TransientId as well
    /// </summary>
    /// <param name="value">Clients to be added</param>
    /// <param name="cancellationToken"></param>
    public async Task AddAsync(ClientCacheDto value, CancellationToken cancellationToken)
    {
        await valkeyCacheService.SetAsync(value.ConnectionId.ToString(), value, cancellationToken, shouldExipire: false);

        foreach (var transientId in value.TransientIds.Select(kvp => kvp.Value))
        {
            await valkeyCacheService.SetAsync(transientId, value.ConnectionId, cancellationToken, shouldExipire: false);
        }
    }
    public async Task<ClientCacheDto?> GetByTransientIdAsync(string transientId, CancellationToken cancellationToken)
    {
        var connectionIdStr = await valkeyCacheService.GetAsync<string>(transientId, null, cancellationToken);

        if (connectionIdStr is not null && Guid.TryParse(connectionIdStr, out var connectionId))
        {
            return await GetAsync(connectionId, cancellationToken);
        }
        return null;
    }

    public async Task UpdateAsync(ClientCacheDto cacheDto, CancellationToken cancellationToken)
    {
        await RemoveAsync(cacheDto.ConnectionId, cancellationToken);
        await AddAsync(cacheDto, cancellationToken);
    }

    /// <summary>
    /// Get a client from the cache by Clients ConnectionId
    /// </summary>
    /// <param name="id">ConnectionID for Clients <see cref="ClientCacheDto"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<ClientCacheDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return valkeyCacheService.GetAsync<ClientCacheDto>(id.ToString(), null, cancellationToken);
    }

    /// <summary>
    /// Remove a client from the cache by Clients ConnectionId
    /// </summary>
    /// <param name="id">ConnectionId for Clients <see cref="ClientCacheDto"/></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await GetAsync(id, cancellationToken);
        if (client is null)
            throw new NullReferenceException("Clients not found in cache when attempting to remove");

        await valkeyCacheService.RemoveAsync(id.ToString(), cancellationToken);

        foreach (var transientId in client.TransientIds.Values)
        {
            await RemoveTransientMappingAsync(transientId, cancellationToken);
        }

    }

    private async Task RemoveTransientMappingAsync(string transientId, CancellationToken cancellationToken)
    {
        await valkeyCacheService.RemoveAsync(transientId, cancellationToken);
    }
}