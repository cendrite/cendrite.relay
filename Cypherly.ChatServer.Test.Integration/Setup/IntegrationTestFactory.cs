using Cypherly.ChatServer.Infrastructure.Settings;
using Cypherly.ChatServer.Test.Integration.Setup.Authentication;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Cypherly.ChatServer.Test.Integration.Setup;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithCleanUp(true)
        .Build();

    private const int ValkeyPort = 6976;
    private readonly IContainer _valkeyContainer = new ContainerBuilder()
        .WithImage("valkey/valkey:latest")
        .WithEnvironment("ALLOW_EMPTY_PASSWORD", "yes")
        .WithExposedPort(ValkeyPort)
        .WithPortBinding(ValkeyPort, 6379)
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {

            #region Database Extensions
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(),
                    b => b.MigrationsAssembly(typeof(TDbContext).Assembly.FullName));
            });

            #endregion

            #region Auth Extensions

            // Mock out authentication and authorization for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("User", policy => policy.RequireAssertion(_ => true));

            #endregion

            #region RabbitMq Extensions

            var rmqDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBusControl));

            if (rmqDescriptor is not null)
                services.Remove(rmqDescriptor);

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumers(typeof(TProgram).Assembly);
            });

            #endregion

            #region Valkey Extensions

            services.Configure<ValkeySettings>(options =>
            {
                options.Host = "localhost"; // The test container's host
                options.Port = ValkeyPort; // The mapped port for the Valkey container
            });

            #endregion

        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _valkeyContainer.StartAsync();
    }

    public async new Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _valkeyContainer.StopAsync();
    }
}