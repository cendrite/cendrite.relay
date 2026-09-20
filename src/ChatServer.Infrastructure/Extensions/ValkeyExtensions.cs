using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ChatServer.Infrastructure.Extensions;

internal static class ValkeyExtensions
{
    internal static void AddValkey(this IServiceCollection services)
    {
        Console.WriteLine("Adding Valkey configuration");

        services.AddSingleton<IDistributedCache>(provider =>
        {
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            return new RedisCache(new RedisCacheOptions()
            {
                ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer),
            });
        });
    }
}
