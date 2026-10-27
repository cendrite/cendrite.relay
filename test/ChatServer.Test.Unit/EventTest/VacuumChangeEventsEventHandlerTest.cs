using System.Text.Json;
using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.ChangeEvent.Base;
using ChatServer.Application.Features.Client.Events;
using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;
using FakeItEasy;
using FluentAssertions;
using ChangeEventDto = ChatServer.Application.Features.ChangeEvent.Base.ChangeEvent;

namespace ChatServer.Test.Unit.EventTest;

public class VacuumChangeEventsEventHandlerTest
{
    private const string ChangeEventTransientId = "change-event-hub-transient-id";

    private readonly IClientRepository _fakeClientRepo;
    private readonly IEventRepository _fakeEventRepo;
    private readonly IChangeEventNotifier _fakeNotifier;
    private readonly IUnitOfWork _fakeUow;
    private readonly VacuumChangeEventsEventHandler _sut;

    private readonly Guid _connectionId = Guid.NewGuid();

    public VacuumChangeEventsEventHandlerTest()
    {
        _fakeClientRepo = A.Fake<IClientRepository>();
        _fakeEventRepo = A.Fake<IEventRepository>();
        _fakeNotifier = A.Fake<IChangeEventNotifier>();
        _fakeUow = A.Fake<IUnitOfWork>();

        _sut = new VacuumChangeEventsEventHandler(_fakeClientRepo, _fakeEventRepo, _fakeNotifier, _fakeUow);
    }

    private VacuumChangeEventsEvent VacuumEvent() => new(_connectionId);

    private Event Event() => new(
        Guid.NewGuid(),
        _connectionId,
        ChangeEventType.FriendRequestSent.ToString(),
        "FriendRequest",
        "Received a friend request",
        DateTime.UtcNow.AddMinutes(-5),
        """{"tag":"friend#1234"}""");

    private Client ClientWith(params string[] hubTypes)
    {
        var client = new Client(Guid.NewGuid(), _connectionId, Guid.NewGuid());

        foreach (var hubType in hubTypes)
            client.AddConnection(new Connection(ChangeEventTransientId, client.Id, hubType));

        return client;
    }

    private void ClientResolvesTo(Client? client) =>
        A.CallTo(() => _fakeClientRepo.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(client);

    private void RepositoryReturns(params IReadOnlyCollection<Event>[] batches) =>
        A.CallTo(() => _fakeEventRepo.GetBatchEventsAsync(_connectionId, A<int>._, A<PageCursor?>._, A<CancellationToken>._))
            .ReturnsNextFromSequence(batches);

    [Fact]
    public async Task Handle_Client_Not_Found_Should_Not_Notify_Or_Delete()
    {
        // Arrange
        ClientResolvesTo(null);

        // Act
        await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeNotifier.NotifyAsync(A<string>._, A<ChangeEventDto>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeEventRepo.GetBatchEventsAsync(A<Guid>._, A<int>._, A<PageCursor?>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Client_Without_ChangeEvent_Hub_Connection_Should_Not_Notify_Or_Delete()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        // Act
        await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeNotifier.NotifyAsync(A<string>._, A<ChangeEventDto>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeEventRepo.DeleteRange(A<IEnumerable<Event>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Should_Restore_Held_Event_With_Original_Id_Timestamp_And_Payload()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.ChangeEvent.ToString()));

        var pending = Event();
        IReadOnlyCollection<Event> batch = [pending];
        RepositoryReturns(batch);

        ChangeEventDto? sent = null;
        A.CallTo(() => _fakeNotifier.NotifyAsync(ChangeEventTransientId, A<ChangeEventDto>._, A<CancellationToken>._))
            .Invokes((string _, ChangeEventDto e, CancellationToken _) => sent = e);

        // Act
        await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        sent.Should().NotBeNull();
        sent!.Id.Should().Be(pending.Id);
        sent.Type.Should().Be(ChangeEventType.FriendRequestSent);
        sent.OccurredOn.Should().Be(pending.OccurredOn);
        sent.Data.Should().BeOfType<JsonElement>()
            .Which.GetProperty("tag").GetString().Should().Be("friend#1234");
    }

    [Fact]
    public async Task Handle_Should_Delete_Batch_And_Save_After_Notifying()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.ChangeEvent.ToString()));

        IReadOnlyCollection<Event> batch = [Event()];
        RepositoryReturns(batch);

        // Act
        await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeNotifier.NotifyAsync(A<string>._, A<ChangeEventDto>._, A<CancellationToken>._))
            .MustHaveHappened()
            .Then(A.CallTo(() => _fakeEventRepo.DeleteRange(batch)).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task Handle_Should_Drain_Batches_Advancing_The_Cursor()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.ChangeEvent.ToString()));

        var full = Enumerable.Range(0, 10).Select(_ => Event()).ToArray();
        var last = full[^1];

        IReadOnlyCollection<Event> first = full;
        IReadOnlyCollection<Event> second = [Event()];
        RepositoryReturns(first, second);

        // Act
        await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeNotifier.NotifyAsync(A<string>._, A<ChangeEventDto>._, A<CancellationToken>._)).MustHaveHappened(11, Times.Exactly);
        A.CallTo(() => _fakeEventRepo.GetBatchEventsAsync(_connectionId, 10, new PageCursor(last.Created, last.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEventRepo.DeleteRange(A<IEnumerable<Event>>._)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task Handle_When_Notify_Fails_Should_Not_Delete_The_Batch()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.ChangeEvent.ToString()));

        IReadOnlyCollection<Event> batch = [Event()];
        RepositoryReturns(batch);

        A.CallTo(() => _fakeNotifier.NotifyAsync(A<string>._, A<ChangeEventDto>._, A<CancellationToken>._)).Throws<Exception>();

        // Act
        var act = async () => await _sut.Handle(VacuumEvent(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        A.CallTo(() => _fakeEventRepo.DeleteRange(A<IEnumerable<Event>>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }
}
