using ChatServer.Infrastructure.Settings;
using ChatServer.Test.Integration.Setup.Authentication;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quartz;
using Testcontainers.PostgreSql;

namespace ChatServer.Test.Integration.Setup;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private readonly string StorageDirectory = Path.Combine(Directory.GetCurrentDirectory());

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:latest")
        .WithCleanUp(true)
        .Build();

    private const int ValkeyPort = 6976;
    private readonly IContainer _valkeyContainer = new ContainerBuilder("valkey/valkey:latest")
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

            services.Configure<QuartzOptions>(options =>
                options["quartz.dataSource.default.connectionString"] = _dbContainer.GetConnectionString());

            // Mock out authentication and authorization for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("User", policy => policy.RequireAssertion(_ => true));

            var rmqDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBusControl));

            if (rmqDescriptor is not null)
                services.Remove(rmqDescriptor);

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumers(typeof(TProgram).Assembly);
            });


            services.Configure<ValkeySettings>(options =>
            {
                options.Host = "localhost"; // The test container's host
                options.Port = ValkeyPort; // The mapped port for the Valkey container
            });

            SetupBlobstore(services);
        });
    }

    private void SetupBlobstore(IServiceCollection services)
    {
        Directory.CreateDirectory(StorageDirectory);

        services.RemoveAll(typeof(IConfigureOptions<BlobStoreSettings>));

        var inMemorySettings = new Dictionary<string, string>
        {
            { "BlobStore:Root",  StorageDirectory }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        services.Configure<BlobStoreSettings>(configuration.GetSection("BlobStore"));
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _valkeyContainer.StartAsync();

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString(),
                b => b.MigrationsAssembly(typeof(TDbContext).Assembly.FullName))
            .Options;

        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
        await context.Database.MigrateAsync();
    }

    public async new Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _valkeyContainer.StopAsync();
    }
}
