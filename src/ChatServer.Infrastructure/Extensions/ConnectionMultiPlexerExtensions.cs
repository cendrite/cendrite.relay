using ChatServer.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ChatServer.Infrastructure.Extensions;

internal static class ConnectionMultiPlexerExtensions
{
    internal static void AddConnectionMultiplexer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ValkeySettings>(configuration.GetSection("Valkey"));

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ConnectionMultiplexer");

            var valkeySettings = provider
                .GetRequiredService<IOptions<ValkeySettings>>()
                .Value;

            var config = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                EndPoints = { $"{valkeySettings.Host}:{valkeySettings.Port}" },
                ChannelPrefix = RedisChannel.Literal("Cendrite.ChatServer.API_"),
            };

            var multiplexer = ConnectionMultiplexer.Connect(config);

            multiplexer.ConnectionFailed += (_, e) =>
                logger.LogWarning(e.Exception,
                    "Connection to Valkey failed");

            multiplexer.ConnectionRestored += (_, e) =>
                logger.LogInformation("Connection to Valkey restored");

            logger.LogInformation("Connected to Valkey");
            return multiplexer;
        });
    }
}