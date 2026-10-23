using System.Reflection;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Infrastructure.Services;
using ChatServer.Infrastructure.Settings;
using ChatServer.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructure(this IServiceCollection svc, IConfiguration cfg, Assembly asm)
    {
        svc.AddJobs(cfg, asm);
        svc.ConfigureSettings(cfg);
        svc.AddPersistence(cfg, asm);
        svc.AddConnectionMultiplexer(cfg);
        svc.AddMassTransitWithRabbitMq(typeof(FriendDeletedConsumer).Assembly);
        svc.AddValkey();
        svc.AddServices();
    }

    private static void AddServices(this IServiceCollection svc)
    {
        svc.AddSingleton<IBlobStore, BlobStore>();
        svc.AddSingleton<IMediaService, MediaService>();
    }

    private static void ConfigureSettings(this IServiceCollection svc, IConfiguration cfg)
    {
        svc.Configure<ValkeySettings>(cfg.GetSection("Valkey"));
        svc.Configure<RabbitMqSettings>(cfg.GetSection("RabbitMq"));
        svc.Configure<BlobStoreSettings>(cfg.GetSection("BlobStore"));
    }
}
