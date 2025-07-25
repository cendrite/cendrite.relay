using Cypherly.ChatServer.Infrastructure.Caching;
using Cypherly.ChatServer.Infrastructure.Persistence.Context;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable MemberCanBePrivate.Global

namespace Cypherly.ChatServer.Test.Integration.Setup;

/// <summary>
/// <see cref="IntegrationTestFactory{TProgram,TDbContext}"/>
/// </summary>
[Collection("ChatServerApplication")]
public class IntegrationTestbase : IDisposable
{
    private readonly IntegrationTestFactory<Program, ChatServerDbContext> _factory;
    protected readonly ChatServerDbContext Db;
    protected readonly HttpClient Client;
    protected readonly ITestHarness Harness;
    protected readonly IValkeyCacheService Cache;

    public IntegrationTestbase(IntegrationTestFactory<Program, ChatServerDbContext> factory)
    {
        Harness = factory.Services.GetTestHarness();
        var scope = factory.Services.CreateScope();
        Db = scope.ServiceProvider.GetRequiredService<ChatServerDbContext>();
        Cache = scope.ServiceProvider.GetRequiredService<IValkeyCacheService>();
        Db.Database.EnsureCreated();
        Client = factory.CreateClient();
        Harness.Start();

    }

    /// <inheritdoc />
    public void Dispose()
    {
        Db.ChatMessage.ExecuteDelete();
        Db.Client.ExecuteDelete();
        Db.OutboxMessage.ExecuteDelete();
        Harness.Stop();
    }
}