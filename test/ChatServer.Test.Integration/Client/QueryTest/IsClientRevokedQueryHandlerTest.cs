using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Features.Client.Queries.IsRevoked;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Integration.Client.QueryTest;

public class IsClientRevokedQueryHandlerTest : IntegrationTestbase
{
    private readonly IsClientRevokedQueryHandler _sut;

    public IsClientRevokedQueryHandlerTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IClientRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IsClientRevokedQueryHandler>>();
        _sut = new IsClientRevokedQueryHandler(repo, logger);
    }

    [Fact]
    public async Task Handle_Given_Active_Client_Should_Return_False()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new IsClientRevokedQuery { ClientId = client.Id }, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Given_Deleted_Client_Should_Return_True()
    {
        // Arrange
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());
        client.SetDelete();
        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new IsClientRevokedQuery { ClientId = client.Id }, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Given_Missing_Client_Should_Return_True()
    {
        // Act
        var result = await _sut.Handle(new IsClientRevokedQuery { ClientId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}
