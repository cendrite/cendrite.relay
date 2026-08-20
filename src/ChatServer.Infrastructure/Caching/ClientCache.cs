using ChatServer.Application.Cache.Client;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Infrastructure.Interfaces;

namespace ChatServer.Infrastructure.Caching;

public class ClientCache(IValkeyCacheService valkeyCacheService) : IClientCache
{
    /// <summary>
    /// Atomically adds a single hub's transient id to a client's hash entry, only if that hub type
    /// doesn't already have one. Also writes the inverse mapping of TransientId to ConnectionId.
    /// Creates the client's hash entry if this is its first hub connection.
    /// </summary>
    /// <returns>false if the hub type already had a transient id (no write happened)</returns>
    public async Task<bool> TryAddTransientIdAsync(Guid connectionId, HubType hubType, string transientId, CancellationToken cancellationToken)
    {
        var added = await valkeyCacheService.HashSetAsync(connectionId.ToString(), hubType.ToString(), transientId);

        if (added)
        {
            await valkeyCacheService.SetAsync(transientId, connectionId, cancellationToken, shouldExipire: false);
        }

        return added;
    }

    /// <summary>
    /// Get a client from the cache by Clients ConnectionId
    /// </summary>
    public async Task<ClientCacheDto?> GetAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var fields = await valkeyCacheService.HashGetAllAsync(connectionId.ToString());

        if (fields.Count == 0) return null;

        var transientIds = fields.ToDictionary(
            kvp => Enum.Parse<HubType>(kvp.Key),
            kvp => kvp.Value);

        return new ClientCacheDto(connectionId, transientIds);
    }

    public async Task<ClientCacheDto?> GetByTransientIdAsync(string transientId, CancellationToken cancellationToken)
    {
        var connectionId = await valkeyCacheService.GetAsync<Guid?>(transientId, null, cancellationToken);

        return connectionId is null ? null : await GetAsync(connectionId.Value, cancellationToken);
    }

    /// <summary>
    /// Remove a client from the cache entirely, along with every hub's transient-id mapping.
    /// </summary>
    public async Task RemoveAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        var client = await GetAsync(connectionId, cancellationToken);
        if (client is null) return;

        await valkeyCacheService.RemoveAsync(connectionId.ToString(), cancellationToken);

        foreach (var transientId in client.TransientIds.Values)
        {
            await valkeyCacheService.RemoveAsync(transientId, cancellationToken);
        }
    }

    /// <summary>
    /// Remove a single hub's transient id from a client's hash entry, without touching any other
    /// hub's entry. Deletes the client's hash entry entirely once its last field is removed.
    /// </summary>
    public async Task RemoveAsync(Guid connectionId, HubType hubType, string transientId, CancellationToken cancellationToken)
    {
        await valkeyCacheService.HashDeleteAsync(connectionId.ToString(), hubType.ToString());
        await valkeyCacheService.RemoveAsync(transientId, cancellationToken);

        var remaining = await valkeyCacheService.HashLengthAsync(connectionId.ToString());
        if (remaining == 0)
        {
            await valkeyCacheService.RemoveAsync(connectionId.ToString(), cancellationToken);
        }
    }
}
