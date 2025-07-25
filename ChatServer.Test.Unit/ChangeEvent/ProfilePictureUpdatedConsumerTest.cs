using AutoFixture;
using ChatServer.Application.Cache.Client;
using ChatServer.Application.Contracts;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Handlers;
using ChatServer.Domain.Aggregates;
using Cypherly.Message.Contracts.Messages.Profile;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ChatServer.Test.Unit.ChangeEvent;

public class ProfilePictureUpdatedConsumerTest
{
    private readonly Fixture _fixture = new();
    private readonly IChangeEventNotifier _fakeChangeEventNotifier;
    private readonly IClientCache _fakeClientCache;

    private readonly ProfilePictureUpdatedConsumer _sut;

    public ProfilePictureUpdatedConsumerTest()
    {
        _fakeChangeEventNotifier = A.Fake<IChangeEventNotifier>();
        _fakeClientCache = A.Fake<IClientCache>();
        var fakeLogger = A.Fake<ILogger<ProfilePictureUpdatedConsumer>>();

        _sut = new ProfilePictureUpdatedConsumer(_fakeChangeEventNotifier, _fakeClientCache, fakeLogger);
    }

    [Fact]
    public async Task Consume_Given_Valid_ConnectedClient_Should_NotifyClient()
    {
        // Arrange
        var client = new Client(Guid.NewGuid(), Guid.NewGuid());
        var profilePictureUpdatedMessage = _fixture.Build<ProfilePictureUpdatedMessage>()
            .With(c => c.ConnectionIds, [client.ConnectionId]).Create();

        var clientCacheDto = new ClientCacheDto(client.ConnectionId, new Dictionary<HubType, string>()
        {
            { HubType.ChangeEvent, "transientId" }
        });

        A.CallTo(() => _fakeClientCache.GetAsync(client.ConnectionId, CancellationToken.None)).Returns(clientCacheDto);

        var fakeConsumeContext = A.Fake<ConsumeContext<ProfilePictureUpdatedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(profilePictureUpdatedMessage);

        // Act
        await _sut.Consume(fakeConsumeContext);

        // Assert
        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync(clientCacheDto.GetTransientId(HubType.ChangeEvent)!,
                A<global::ChatServer.Application.Features.ChangeEvent.Base.ChangeEvent>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Consume_Given_No_Clients_Should_Not_Notify()
    {
        var profilePictureUpdatedMessage = _fixture.Create<ProfilePictureUpdatedMessage>();
        var fakeConsumeContext = A.Fake<ConsumeContext<ProfilePictureUpdatedMessage>>();
        A.CallTo(() => fakeConsumeContext.Message).Returns(profilePictureUpdatedMessage);

        await _sut.Consume(fakeConsumeContext);

        A.CallTo(() => _fakeChangeEventNotifier.NotifyAsync(A<string>._, A<global::ChatServer.Application.Features.ChangeEvent.Base.ChangeEvent>._, A<CancellationToken>._))
            .MustNotHaveHappened();

    }
}