using ChatServer.Infrastructure.Interfaces;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup.Authentication;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable MemberCanBePrivate.Global

namespace ChatServer.Test.Integration.Setup;

/// <summary>
/// <see cref="IntegrationTestFactory{TProgram,TDbContext}"/>
/// </summary>
[Collection("ChatServerApplication")]
public class IntegrationTestbase : IDisposable
{
    protected readonly IntegrationTestFactory<Program, ChatServerDbContext> Factory;
    protected readonly ChatServerDbContext Db;
    protected readonly ITestHarness Harness;
    protected readonly IValkeyCacheService Cache;

    public IntegrationTestbase(IntegrationTestFactory<Program, ChatServerDbContext> factory)
    {
        Factory = factory;
        Harness = factory.Services.GetTestHarness();
        var scope = factory.Services.CreateScope();
        Db = scope.ServiceProvider.GetRequiredService<ChatServerDbContext>();
        Cache = scope.ServiceProvider.GetRequiredService<IValkeyCacheService>();
        Db.Database.EnsureCreated();
        Harness.Start();
    }

    public void Dispose()
    {
        Db.ChatMessage.ExecuteDelete();
        Db.Client.ExecuteDelete();
        Db.OutboxMessage.ExecuteDelete();
        Harness.Stop();
    }

    public HttpClient CreateClient(string tenantId, string? deviceId = null)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, tenantId);
        if (deviceId is not null)
            client.DefaultRequestHeaders.Add(TestAuthHandler.DeviceIdHeader, deviceId);
        return client;
    }
}
