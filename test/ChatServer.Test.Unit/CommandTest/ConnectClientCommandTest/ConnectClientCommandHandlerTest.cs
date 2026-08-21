using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Connect;
using ChatServer.Domain.Aggregates;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.CommandTest.ConnectClientCommandTest;

public class ConnectClientCommandHandlerTest
{
    private readonly IClientRepository _fakeRepository;
    private readonly IUnitOfWork _fakeUow;
    private readonly ConnectClientCommandHandler _sut;

    public ConnectClientCommandHandlerTest()
    {
        _fakeRepository = A.Fake<IClientRepository>();
        _fakeUow = A.Fake<IUnitOfWork>();
        _sut = new ConnectClientCommandHandler(_fakeRepository, _fakeUow, A.Fake<ILogger<ConnectClientCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Invalid_ClientId_Should_Return_Result_Fail()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).Returns((Client)null!);

        var command = new ConnectClientCommand()
        {
            ClientId = clientId,
            TransientId = "test",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Valid_ClientId_Should_Return_Result_Ok()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client(clientId, Guid.NewGuid());

        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).Returns(client);

        var command = new ConnectClientCommand()
        {
            ClientId = clientId,
            TransientId = "test",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        client.Connections.Should().ContainSingle(c => c.TransientId == command.TransientId && c.Type == HubType.ChangeEvent.ToString());
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Existing_HubType_Should_Replace_Connection_And_Return_Result_Ok()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client(clientId, Guid.NewGuid());
        client.AddConnection(new Connection("old-transient-id", clientId, HubType.ChangeEvent.ToString()));

        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).Returns(client);

        var command = new ConnectClientCommand()
        {
            ClientId = clientId,
            TransientId = "new-transient-id",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        client.Connections.Should().ContainSingle(c => c.TransientId == command.TransientId);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Exception_Should_Return_Result_Fail()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client(clientId, Guid.NewGuid());

        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).Returns(client);
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).Throws<Exception>();

        var command = new ConnectClientCommand()
        {
            ClientId = clientId,
            TransientId = "test",
            HubType = HubType.ChangeEvent
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetByIdAsync(clientId, CancellationToken.None)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        result.Success.Should().BeFalse();
    }
}
