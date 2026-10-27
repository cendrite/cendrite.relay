using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Connect;
using ChatServer.Domain.Common;
using ChatServer.Domain.Events.Client;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ConnectClientCommandHandler>>();
        _sut = new ConnectClientCommandHandler(repo, unitOfWork, logger);
    }

    [Fact]
    public async Task Handle_Invalid_ClientId_Should_Return_Result_Fail() {
        // Arrange
        var command = new ConnectClientCommand()
        {
            ClientId = Guid.NewGuid(),
            TransientId = "TransientId",
            HubType = HubType.ChangeEvent };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Deleted_Client_Should_Return_Revoked_And_Not_Connect()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        client.SetDelete();

        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        var command = new ConnectClientCommand()
        {
            ClientId = client.Id,
            TransientId = "TransientId",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(Error.Revoked());
        Db.OutboxMessage.Any().Should().BeFalse();
        Db.Client.AsNoTracking().Include(c => c.Connections).Single(c => c.Id == client.Id).Connections.Should().BeEmpty();
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
        Db.OutboxMessage.Select(o => o.Type).AsEnumerable().Should().HaveCount(2)
            .And.Contain(t => t.StartsWith(typeof(ClientConnectedEvent).FullName!))
            .And.Contain(t => t.StartsWith(typeof(VacuumChangeEventsEvent).FullName!));


        var updatedClient = await Db.Client.AsNoTracking().Include(c => c.Connections).FirstOrDefaultAsync(c=> c.Id  == client.Id);
        updatedClient.Should().NotBeNull();
        updatedClient!.Connections.FirstOrDefault(c=> c.ClientId  == client.Id).Should().NotBeNull();
    }
}