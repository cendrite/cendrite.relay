using System.Text.Json;

namespace ChatServer.Infrastructure.Interfaces;

public interface IValkeyCacheService
{
    Task<bool> HashSetAsync(string key, string field, string value);
    Task<bool> HashDeleteAsync(string key, string field);
    Task<Dictionary<string, string>> HashGetAllAsync(string key);
    Task<long> HashLengthAsync(string key);
    Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken ct);
    Task SetAsync<T>(string key, T value, CancellationToken ct, bool shouldExipire = true, TimeSpan? expiry = null);
    Task RemoveAsync(string key, CancellationToken ct);
}
