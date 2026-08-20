using System.Text.Json;
using ChatServer.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ChatServer.Infrastructure.Caching;

internal class ValkeyCacheService(IDistributedCache cache, IConnectionMultiplexer connection) : IValkeyCacheService
{
    public IDatabase Database => connection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken ct)
    {
        var serializedValue = await cache.GetStringAsync(key, ct);

        return serializedValue == null ? default : JsonSerializer.Deserialize<T>(serializedValue, options ?? null);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken ct, bool shouldExipire = true, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions();

        if (shouldExipire)
        {
            options.AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5);
        }

        var serializedValue = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, serializedValue, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
        => await cache.RemoveAsync(key, ct);

    public async Task<bool> HashSetAsync(string key, string field, string value)
        => await Database.HashSetAsync(key, field, value, When.NotExists);

    public async Task<bool> HashDeleteAsync(string key, string field)
        => await Database.HashDeleteAsync(key, field);

    public async Task<long> HashLengthAsync(string key) 
        => await Database.HashLengthAsync(key);

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var entries = await Database.HashGetAllAsync(key);
        return entries.ToDictionary(entry => entry.Name.ToString(), entry => entry.Value.ToString());
    }
}
