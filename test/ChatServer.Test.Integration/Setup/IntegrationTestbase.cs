using ChatServer.Infrastructure.Caching;
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
    private readonly IntegrationTestFactory<Program, ChatServerDbContext> _factory;
    protected readonly ChatServerDbContext Db;
    protected readonly ITestHarness Harness;
    protected readonly IValkeyCacheService Cache;

    public IntegrationTestbase(IntegrationTestFactory<Program, ChatServerDbContext> factory)
    {
        _factory = factory;
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

    public HttpClient CreateClient(string tenantId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantIdHeader, tenantId);
        return client;
    }
}
