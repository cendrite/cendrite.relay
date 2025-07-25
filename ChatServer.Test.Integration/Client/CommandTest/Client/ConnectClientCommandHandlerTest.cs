using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Connect;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Integration.Client.CommandTest.Client;

public class ConnectClientCommandHandlerTest : IntegrationTestbase
{
    private readonly ConnectClientCommandHandler _sut;
    public ConnectClientCommandHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var clientCache = scope.ServiceProvider.GetRequiredService<IClientCache>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConnectClientCommandHandler>>();
        _sut = new ConnectClientCommandHandler(repo, unitOfWork, clientCache, logger);
    }

    [Fact]
    public async Task Handle_Invalid_ClientId_Should_Return_Result_Fail()
    {
        // Arrange
        var command = new ConnectClientCommand()
        {
            ClientId = Guid.NewGuid(),
            TransientId = "TransientId",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Valid_Connect_Command_Should_Connect_Client()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());

        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        var command = new ConnectClientCommand()
        {
            ClientId = client.Id,
            TransientId = "asædajshd12393JK",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        Db.OutboxMessage.Should().HaveCount(1);


        var cachedClient = Cache.GetAsync<Domain.Aggregates.Client>(client.ConnectionId.ToString(), null, CancellationToken.None);
        cachedClient.Should().NotBeNull();
        var inverseMapping = Cache.GetAsync<string>(command.TransientId, null, CancellationToken.None);
        inverseMapping.Should().NotBeNull();
    }
}