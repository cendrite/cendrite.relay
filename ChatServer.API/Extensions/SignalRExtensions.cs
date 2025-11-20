using ChatServer.API.Handlers;
using ChatServer.Application.Contracts.Services;
using StackExchange.Redis;

namespace ChatServer.API.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
    {
        services.AddSignalR()
            .AddStackExchangeRedis(options =>
            {
                ;
                options.ConnectionFactory = async _ =>
                    await Task.FromResult(services.BuildServiceProvider()
                        .GetRequiredService<IConnectionMultiplexer>());
            });

        services.AddSingleton<IChangeEventNotifier, ChangeEventHandler>();
        services.AddSingleton<IMessageSender, MessageHandler>();

        return services;
    }
}
