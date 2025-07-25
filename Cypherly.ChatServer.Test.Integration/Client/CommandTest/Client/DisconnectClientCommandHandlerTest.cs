using Cypherly.ChatServer.Application.Cache.Client;
using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Contracts.Cache;
using Cypherly.ChatServer.Application.Enums;
using Cypherly.ChatServer.Application.Features.Client.Commands.Disconnect;
using Cypherly.ChatServer.Infrastructure.Persistence.Context;
using Cypherly.ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.ChatServer.Test.Integration.Client.CommandTest.Client;

public class DisconnectClientCommandHandlerTest : IntegrationTestbase
{
    private readonly DisconnectClientCommandHandler _sut;
    public DisconnectClientCommandHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IClientCache>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DisconnectClientCommandHandler>>();

        _sut = new DisconnectClientCommandHandler(cache, logger);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldRemoveClientFromCache()
    {
        // Arrange
        const string transientId = "TransientId";
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());

        var clientCacheDto = new ClientCacheDto(client.ConnectionId, new Dictionary<HubType, string>()
        {
            { HubType.ChangeEvent, transientId }
        });

        await Cache.SetAsync(client.ConnectionId.ToString(), clientCacheDto, CancellationToken.None);
        await Cache.SetAsync(transientId, client.ConnectionId.ToString(), CancellationToken.None);

        var command = new DisconnectClientCommand()
        {
            TransientId = transientId,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        var cacheClient = await Cache.GetAsync<Domain.Aggregates.Client>(client.ConnectionId.ToString(), null, CancellationToken.None);
        cacheClient.Should().BeNull();

        var inverseMapping = await Cache.GetAsync<string>(clientCacheDto.GetTransientId(HubType.ChangeEvent)!, null, CancellationToken.None);
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
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }
}