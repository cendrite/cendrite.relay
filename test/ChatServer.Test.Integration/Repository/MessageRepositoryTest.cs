using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.Repository;

public class MessageRepositoryTest : IntegrationTestbase
{
    private readonly IMessageRepository _sut;
    private readonly IUnitOfWork _uow;

    public MessageRepositoryTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _sut = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
        _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    /// <summary>
    /// ChatMessage.RecipientId is a foreign key onto Client.ConnectionId, so a recipient
    /// has to exist before any message addressed to it can be inserted.
    /// </summary>
    private async Task<Guid> SeedRecipientAsync()
    {
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());

        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        return client.ConnectionId;
    }

    private async Task SeedMessagesAsync(params ChatMessage[] messages)
    {
        await Db.ChatMessage.AddRangeAsync(messages);
        await Db.SaveChangesAsync();
    }

    private static ChatMessage Message(Guid recipientId, DateTime timestamp, string content = "ciphertext") =>
        new(Guid.NewGuid(), Guid.NewGuid(), recipientId, "chat-id", null, content, timestamp);

    [Fact]
    public async Task GetBatchMessagesAsync_Should_Return_Oldest_Messages_First()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        await SeedMessagesAsync(
            Message(recipientId, now.AddMinutes(-1), "newest"),
            Message(recipientId, now.AddMinutes(-3), "oldest"),
            Message(recipientId, now.AddMinutes(-2), "middle"));

        // Act
        var messages = await _sut.GetBatchMessagesAsync(recipientId, 10, ct: CancellationToken.None);

        // Assert
        messages.Select(m => m.Content).Should().ContainInOrder("oldest", "middle", "newest");
    }

    [Fact]
    public async Task GetBatchMessagesAsync_Should_Only_Return_Messages_For_The_Given_Recipient()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var otherRecipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        await SeedMessagesAsync(
            Message(recipientId, now, "mine"),
            Message(otherRecipientId, now, "theirs"));

        // Act
        var messages = await _sut.GetBatchMessagesAsync(recipientId, 10, ct: CancellationToken.None);

        // Assert
        messages.Should().ContainSingle();
        messages.Single().Content.Should().Be("mine");
        messages.Single().RecipientId.Should().Be(recipientId);
    }

    [Fact]
    public async Task GetBatchMessagesAsync_Should_Respect_The_Batch_Count()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        var messages = Enumerable.Range(0, 25)
            .Select(i => Message(recipientId, now.AddSeconds(i), $"message {i}"))
            .ToArray();

        await SeedMessagesAsync(messages);

        // Act
        var batch = await _sut.GetBatchMessagesAsync(recipientId, 10, ct: CancellationToken.None);

        // Assert
        batch.Should().HaveCount(10);
        batch.Select(m => m.Content).Should().ContainInOrder("message 0", "message 9");
    }

    [Fact]
    public async Task GetBatchMessagesAsync_Given_No_Pending_Messages_Should_Return_Empty()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();

        // Act
        var messages = await _sut.GetBatchMessagesAsync(recipientId, 10, ct: CancellationToken.None);

        // Assert
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteRange_Should_Remove_Only_The_Given_Messages()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        var first = Message(recipientId, now.AddMinutes(-2), "first");
        var second = Message(recipientId, now.AddMinutes(-1), "second");
        await SeedMessagesAsync(first, second);

        var batch = await _sut.GetBatchMessagesAsync(recipientId, 1, ct: CancellationToken.None);

        // Act
        _sut.DeleteRange(batch);
        await _uow.SaveChangesAsync(CancellationToken.None);

        // Assert
        var remaining = await Db.ChatMessage.AsNoTracking().ToListAsync();
        remaining.Should().ContainSingle();
        remaining.Single().Content.Should().Be("second");
    }

    [Fact]
    public async Task GetBatchMessagesAsync_Given_A_Cursor_Should_Continue_After_It()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        await SeedMessagesAsync(Enumerable.Range(0, 5)
            .Select(i => Message(recipientId, now.AddSeconds(i), $"message {i}"))
            .ToArray());

        // Act
        var first = await _sut.GetBatchMessagesAsync(recipientId, 2, ct: CancellationToken.None);
        var last = first.Last();
        var second = await _sut.GetBatchMessagesAsync(recipientId, 2, new MessageCursor(last.Timestamp, last.Id), CancellationToken.None);

        // Assert
        first.Select(m => m.Content).Should().ContainInOrder("message 0", "message 1");
        second.Select(m => m.Content).Should().ContainInOrder("message 2", "message 3");
    }

    /// <summary>
    /// The reason the cursor carries Id as well as Timestamp: a page boundary landing inside a
    /// run of identical timestamps must not skip or repeat rows.
    /// </summary>
    [Fact]
    public async Task GetBatchMessagesAsync_Given_Identical_Timestamps_Should_Not_Skip_Or_Repeat()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var timestamp = DateTime.UtcNow;

        await SeedMessagesAsync(Enumerable.Range(0, 4)
            .Select(i => Message(recipientId, timestamp, $"message {i}"))
            .ToArray());

        // Act
        var first = await _sut.GetBatchMessagesAsync(recipientId, 2, ct: CancellationToken.None);
        var last = first.Last();
        var second = await _sut.GetBatchMessagesAsync(recipientId, 2, new MessageCursor(last.Timestamp, last.Id), CancellationToken.None);

        // Assert
        first.Should().HaveCount(2);
        second.Should().HaveCount(2);
        first.Select(m => m.Id).Should().NotIntersectWith(second.Select(m => m.Id));
    }
}
