using ChatServer.Application.Cache.Client;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Disconnect;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Integration.Client.CommandTest.Client;

public class DisconnectClientCommandHandlerTest : IntegrationTestbase
{
    private readonly DisconnectClientCommandHandler _sut;
    private readonly IClientCache _clientCache;
    public DisconnectClientCommandHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _clientCache = scope.ServiceProvider.GetRequiredService<IClientCache>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DisconnectClientCommandHandler>>();

        _sut = new DisconnectClientCommandHandler(_clientCache, logger);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldRemoveClientFromCache()
    {
        // Arrange
        const string transientId = "TransientId";
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());

        await _clientCache.TryAddTransientIdAsync(client.ConnectionId, HubType.ChangeEvent, transientId, CancellationToken.None);

        var command = new DisconnectClientCommand()
        {
            TransientId = transientId,
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        var cacheClient = await _clientCache.GetAsync(client.ConnectionId, CancellationToken.None);
        cacheClient.Should().BeNull();

        var inverseMapping = await Cache.GetAsync<Guid?>(transientId, null, CancellationToken.None);
        inverseMapping.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenClientDoesNotExist_Should_Return_Result_Fail()
    {
        // Arrange
        const string transientId = "TransientId";
        var command = new DisconnectClientCommand()
        {
            TransientId = transientId,
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }
}