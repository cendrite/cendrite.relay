using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Queries.GetPresence;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.Client.QueryTest;

public class GetPresenceQueryHandlerTest : IntegrationTestbase
{
    private readonly GetPresenceQueryHandler _sut;

    public GetPresenceQueryHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        _sut = new GetPresenceQueryHandler(repo);
    }

    [Fact]
    public async Task Handle_Should_Return_Presence_For_Users_Active_Clients_Only()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var online = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), userId);
        online.AddConnection(new Connection("transient-id", online.Id, HubType.ChangeEvent.ToString()));

        var offline = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), userId);

        var deleted = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), userId);
        deleted.SetDelete();

        var otherUsers = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await Db.Client.AddRangeAsync(online, offline, deleted, otherUsers);
        await Db.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new GetPresenceQuery { UserId = userId }, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Devices.Should().HaveCount(2);
        result.Value.Devices.Single(d => d.DeviceId == online.Id).Online.Should().BeTrue();
        result.Value.Devices.Single(d => d.DeviceId == offline.Id).Online.Should().BeFalse();
    }
}
