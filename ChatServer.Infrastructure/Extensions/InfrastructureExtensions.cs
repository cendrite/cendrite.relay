using System.Reflection;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddOutboxProcessingJob(assembly);
        services.ConfigureSettings(configuration);
        services.AddPersistence(configuration, assembly);
        services.AddConnectionMultiplexer(configuration);
        services.AddMassTransitWithRabbitMq(typeof(FriendDeletedConsumer).Assembly);
        services.AddValkey();
        return services;
    }

    private static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ValkeySettings>(configuration.GetSection("Valkey"));
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
    }
}