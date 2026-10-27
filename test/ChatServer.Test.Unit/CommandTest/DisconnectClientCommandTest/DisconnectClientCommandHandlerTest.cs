using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Disconnect;
using ChatServer.Domain.Aggregates;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.CommandTest.DisconnectClientCommandTest;

public class DisconnectClientCommandHandlerTest
{
    private readonly IClientRepository _fakeRepository;
    private readonly IUnitOfWork _fakeUow;
    private readonly DisconnectClientCommandHandler _sut;

    public DisconnectClientCommandHandlerTest()
    {
        _fakeRepository = A.Fake<IClientRepository>();
        _fakeUow = A.Fake<IUnitOfWork>();
        _sut = new DisconnectClientCommandHandler(_fakeRepository, _fakeUow, A.Fake<ILogger<DisconnectClientCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Client_Not_Found_Should_Return_Result_Fail()
    {
        // Arrange
        const string transientId = "test_transient_id";

        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns((Client?)null);

        var command = new DisconnectClientCommand { TransientId = transientId, HubType = HubType.ChangeEvent };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Client_Found_Should_Return_Result_Ok()
    {
        // Arrange
        const string transientId = "test_transient_id";
        var client = new Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        client.AddConnection(new Connection(transientId, client.Id, HubType.ChangeEvent.ToString()));

        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(client);

        var command = new DisconnectClientCommand { TransientId = transientId, HubType = HubType.ChangeEvent };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        client.Connections.Should().BeEmpty();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Exception_Should_Return_Result_Fail()
    {
        // Arrange
        const string transientId = "test_transient_id";
        var client = new Client(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        client.AddConnection(new Connection(transientId, client.Id, HubType.ChangeEvent.ToString()));

        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(client);
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).Throws<Exception>();

        var command = new DisconnectClientCommand { TransientId = transientId, HubType = HubType.ChangeEvent };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        result.Success.Should().BeFalse();
    }
}
