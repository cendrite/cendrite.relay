using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Contracts.Cache;
using Cypherly.ChatServer.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Cypherly.ChatServer.Infrastructure.Extensions;

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
        services.AddScoped<IClientCache, ClientCache>();
        services.AddScoped<IMessageCache, MessageCacheService>();
    }
}