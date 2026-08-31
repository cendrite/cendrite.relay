using AutoFixture;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using Cypherly.Message.Contracts.Messages.Profile;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public class AvatarUpdatedConsumerTest
{
    private readonly Fixture _fixture = new();
    private readonly IChangeEventNotifier _fakeChangeEventNotifier;
    private readonly IClientRepository _fakeClientRepository;

    private readonly AvatarUpdatedConsumer _sut;

    public AvatarUpdatedConsumerTest()
    {
        _fakeChangeEventNotifier = A.Fake<IChangeEventNotifier>();
        _fakeClientRepository = A.Fake<IClientRepository>();
        var fakeLogger = A.Fake<ILogger<AvatarUpdatedConsumer>>();

        _sut = new AvatarUpdatedConsumer(_fakeChangeEventNotifier, _fakeClientRepository, fakeLogger);
    }

    [Fact]
    public async Task Consume_Given_Valid_ConnectedClient_Should_NotifyClient()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), Guid.NewGuid());
        client.AddConnection(new Connection("transientId", client.Id, HubType.ChangeEvent.ToString()));

        var profilePictureUpdatedMessage = _fixture.Build<AvatarUpdatedMessage>()
            .With(c => c.ConnectionIds, [client.ConnectionId]).Create();

        A.CallTo(() => _fakeClientRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._))
            .Returns(client);

        var fakeConsumeContext = A.Fake<ConsumeContext<AvatarUpdatedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(profilePictureUpdatedMessage);

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync("transientId",
                A<Application.Features.ChangeEvent.Base.ChangeEvent>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Consume_Given_No_Clients_Should_Not_Notify()
    {
        var profilePictureUpdatedMessage = _fixture.Create<AvatarUpdatedMessage>();
        var fakeConsumeContext = A.Fake<ConsumeContext<AvatarUpdatedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(profilePictureUpdatedMessage);

        A.CallTo(() => _fakeClientRepository.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._))
            .Returns((Client?)null);

        await _sut.Consume(fakeConsumeContext);

        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync(A<string>._, A<global::ChatServer.Application.Features.ChangeEvent.Base.ChangeEvent>._, A<CancellationToken>._))
            .MustNotHaveHappened();

    }
}
