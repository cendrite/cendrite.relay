using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ChatServer.Infrastructure.Caching;

[SuppressMessage("Design", "CA1068:CancellationToken parameters must come last")]
public interface IValkeyCacheService
{
    Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken, bool shouldExipire = true, TimeSpan? expiry = null);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}

internal class ValkeyCacheService(IDistributedCache cache, IConnectionMultiplexer connection) : IValkeyCacheService
{
    public IDatabase Database => connection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        var serializedValue = await cache.GetStringAsync(key, cancellationToken);

        return serializedValue == null ? default : JsonSerializer.Deserialize<T>(serializedValue, options ?? null);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken, bool shouldExipire = true, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions();

        if (shouldExipire)
        {
            options.AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5);
        }

        var serializedValue = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }
}