using ChatServer.API.Handlers;
using ChatServer.API.Hubs.Filters;
using ChatServer.Application.Contracts.Services;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace ChatServer.API.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
    {
        services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
                options.AddFilter<RevokedClientHubFilter>();
            })
            .AddStackExchangeRedis(options =>
            {
                options.ConnectionFactory = async _ =>
                    await Task.FromResult(services.BuildServiceProvider()
                        .GetRequiredService<IConnectionMultiplexer>());
            });

        services.AddSingleton<IChangeEventNotifier, ChangeEventHandler>();
        services.AddSingleton<IMessageSender, MessageHandler>();

        return services;
    }
}
