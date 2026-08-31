using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Infrastructure.Caching;
using ChatServer.Infrastructure.Interfaces;
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

        services.AddCacheServices();
    }

    private static void AddCacheServices(this IServiceCollection services)
    {
        services.AddSingleton<IValkeyCacheService, ValkeyCacheService>();
        services.AddScoped<IMessageCache, MessageCacheService>();
    }
}
