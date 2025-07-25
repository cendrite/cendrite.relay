using System.Reflection;
using Cypherly.ChatServer.Application.Contracts.Repositories;
using Cypherly.ChatServer.Infrastructure.Persistence.Context;
using Cypherly.ChatServer.Infrastructure.Persistence.Repositories;
using Cypherly.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cypherly.ChatServer.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    private const string ConnectionStringName = "ChatServerDbConnectionString";

    internal static void AddPersistence(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddDbContext<ChatServerDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(ConnectionStringName),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                });
        });

        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
    }
}