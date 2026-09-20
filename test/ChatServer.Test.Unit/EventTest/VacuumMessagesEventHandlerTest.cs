using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Events;
using ChatServer.Application.Features.Messaging.Models;
using ChatServer.Domain.Aggregates;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Events.Client;
using FakeItEasy;
using FluentAssertions;

namespace ChatServer.Test.Unit.EventTest;

public class VacuumMessagesEventHandlerTest
{
    private const string MessageTransientId = "message-hub-transient-id";

    private readonly IClientRepository _fakeClientRepo;
    private readonly IMessageRepository _fakeMessageRepo;
    private readonly IMessageSender _fakeSender;
    private readonly IUnitOfWork _fakeUow;
    private readonly VacuumMessagesEventHandler _sut;

    private readonly Guid _connectionId = Guid.NewGuid();

    public VacuumMessagesEventHandlerTest()
    {
        _fakeClientRepo = A.Fake<IClientRepository>();
        _fakeMessageRepo = A.Fake<IMessageRepository>();
        _fakeSender = A.Fake<IMessageSender>();
        _fakeUow = A.Fake<IUnitOfWork>();

        _sut = new VacuumMessagesEventHandler(_fakeClientRepo, _fakeMessageRepo, _fakeSender, _fakeUow);
    }

    private VacuumMessagesEvent Event() => new(_connectionId);

    private int _sequence;

    private ChatMessage Message() =>
        new(Guid.NewGuid(), Guid.NewGuid(), _connectionId, "chat-id", null, "ciphertext",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(_sequence++));

    private Client ClientWith(params string[] hubTypes)
    {
        var client = new Client(Guid.NewGuid(), _connectionId);

        foreach (var hubType in hubTypes)
            client.AddConnection(new Connection(MessageTransientId, client.Id, hubType));

        return client;
    }

    private void ClientResolvesTo(Client? client) =>
        A.CallTo(() => _fakeClientRepo.GetAsync(A<ISpecification<Client>>._, A<CancellationToken>._)).Returns(client);

    private void RepositoryReturns(params IReadOnlyCollection<ChatMessage>[] batches) =>
        A.CallTo(() => _fakeMessageRepo.GetBatchMessagesAsync(_connectionId, A<int>._, A<MessageCursor?>._, A<CancellationToken>._))
            .ReturnsNextFromSequence(batches);

    [Fact]
    public async Task Handle_Client_Not_Found_Should_Not_Send_Or_Delete()
    {
        // Arrange
        ClientResolvesTo(null);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeMessageRepo.DeleteRange(A<IEnumerable<ChatMessage>>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeMessageRepo.GetBatchMessagesAsync(A<Guid>._, A<int>._, A<MessageCursor?>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Client_Without_Message_Hub_Connection_Should_Not_Send_Or_Delete()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.ChangeEvent.ToString()));

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeMessageRepo.DeleteRange(A<IEnumerable<ChatMessage>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Should_Send_Every_Message_To_The_Message_Hub_Connection()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        var first = Message();
        var second = Message();
        IReadOnlyCollection<ChatMessage> batch = [first, second];
        IReadOnlyCollection<ChatMessage> empty = [];
        RepositoryReturns(batch, empty);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(MessageTransientId, A<OutgoingMessage>.That.Matches(m => m.Id == first.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeSender.SendMessageAsync(MessageTransientId, A<OutgoingMessage>.That.Matches(m => m.Id == second.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Should_Delete_Batch_And_Save_After_Sending()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        IReadOnlyCollection<ChatMessage> batch = [Message()];
        IReadOnlyCollection<ChatMessage> empty = [];
        RepositoryReturns(batch, empty);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._))
            .MustHaveHappened()
            .Then(A.CallTo(() => _fakeMessageRepo.DeleteRange(batch)).MustHaveHappenedOnceExactly())
            .Then(A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly());
    }

    [Fact]
    public async Task Handle_Should_Drain_Batches_Until_Repository_Returns_Empty()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        // a full page keeps the drain going; the short page ends it
        IReadOnlyCollection<ChatMessage> first = Enumerable.Range(0, 10).Select(_ => Message()).ToArray();
        IReadOnlyCollection<ChatMessage> second = [Message()];
        RepositoryReturns(first, second);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._)).MustHaveHappened(11, Times.Exactly);
        A.CallTo(() => _fakeMessageRepo.DeleteRange(A<IEnumerable<ChatMessage>>._)).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task Handle_No_Pending_Messages_Should_Not_Delete_Or_Save()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        IReadOnlyCollection<ChatMessage> empty = [];
        RepositoryReturns(empty);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeMessageRepo.DeleteRange(A<IEnumerable<ChatMessage>>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_When_Send_Fails_Should_Not_Delete_The_Batch()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        IReadOnlyCollection<ChatMessage> batch = [Message()];
        IReadOnlyCollection<ChatMessage> empty = [];
        RepositoryReturns(batch, empty);

        A.CallTo(() => _fakeSender.SendMessageAsync(A<string>._, A<OutgoingMessage>._, A<CancellationToken>._)).Throws<Exception>();

        // Act
        var act = async () => await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        A.CallTo(() => _fakeMessageRepo.DeleteRange(A<IEnumerable<ChatMessage>>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUow.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_First_Page_Should_Be_Requested_Without_A_Cursor()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        IReadOnlyCollection<ChatMessage> empty = [];
        RepositoryReturns(empty);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeMessageRepo.GetBatchMessagesAsync(_connectionId, 10, null, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Should_Advance_The_Cursor_To_The_Last_Message_Of_The_Previous_Page()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        var full = Enumerable.Range(0, 10).Select(_ => Message()).ToArray();
        var last = full[^1];

        IReadOnlyCollection<ChatMessage> first = full;
        IReadOnlyCollection<ChatMessage> second = [Message()];
        RepositoryReturns(first, second);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeMessageRepo.GetBatchMessagesAsync(
                _connectionId,
                10,
                new MessageCursor(last.Timestamp, last.Id),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Should_Stop_Without_Another_Query_When_A_Page_Is_Not_Full()
    {
        // Arrange
        ClientResolvesTo(ClientWith(HubType.Message.ToString()));

        IReadOnlyCollection<ChatMessage> partial = [Message(), Message()];
        RepositoryReturns(partial);

        // Act
        await _sut.Handle(Event(), CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeMessageRepo.GetBatchMessagesAsync(A<Guid>._, A<int>._, A<MessageCursor?>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeMessageRepo.DeleteRange(partial)).MustHaveHappenedOnceExactly();
    }
}
