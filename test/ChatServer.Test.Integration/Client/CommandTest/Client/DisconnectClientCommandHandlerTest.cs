using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Disconnect;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Integration.Client.CommandTest.Client;

public class DisconnectClientCommandHandlerTest : IntegrationTestbase
{
    private readonly DisconnectClientCommandHandler _sut;
    public DisconnectClientCommandHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DisconnectClientCommandHandler>>();

        _sut = new DisconnectClientCommandHandler(repo, uow, logger);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldRemoveConnection()
    {
        // Arrange
        const string transientId = "TransientId";
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        client.AddConnection(new Connection(transientId, client.Id, nameof(HubType.ChangeEvent)));

        await Db.AddAsync(client);
        await Db.SaveChangesAsync();

        var command = new DisconnectClientCommand()
        {
            TransientId = transientId,
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        var assertClient = await Db.Client.AsNoTracking().Include(c=> c.Connections).FirstOrDefaultAsync(c=> c.Id == client.Id);
        assertClient.Should().NotBeNull();
        assertClient!.Connections.Should().HaveCount(0);
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