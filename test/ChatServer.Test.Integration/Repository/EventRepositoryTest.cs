using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.Repository;

public class EventRepositoryTest : IntegrationTestbase
{
    private readonly IEventRepository _sut;
    private readonly IUnitOfWork _uow;

    public EventRepositoryTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _sut = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    /// <summary>
    /// Event.RecipientId is a foreign key onto Client.ConnectionId, so a recipient
    /// has to exist before any event addressed to it can be inserted.
    /// </summary>
    private async Task<Guid> SeedRecipientAsync()
    {
        var client = new Domain.Aggregates.Client(Guid.NewGuid(), Guid.NewGuid());

        await Db.Client.AddAsync(client);
        await Db.SaveChangesAsync();

        return client.ConnectionId;
    }

    private readonly Dictionary<Guid, DateTime> _created = [];

    private async Task SeedEventsAsync(params Event[] events)
    {
        await Db.Event.AddRangeAsync(events);

        foreach (var evt in events)
            Db.Entry(evt).Property(e => e.Created).CurrentValue = _created[evt.Id];

        await Db.SaveChangesAsync();
    }

    private Event Event(Guid recipientId, DateTime created, string title = "title", string data = """{"tag":"friend#1234"}""")
    {
        var evt = new Event(Guid.NewGuid(), recipientId, "FriendRequestSent", title, "description", created, data);
        _created[evt.Id] = created;
        return evt;
    }

    [Fact]
    public async Task GetBatchEventsAsync_Should_Return_Oldest_Events_First_For_The_Given_Recipient()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var otherRecipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        await SeedEventsAsync(
            Event(recipientId, now.AddMinutes(-1), "newest"),
            Event(recipientId, now.AddMinutes(-3), "oldest"),
            Event(otherRecipientId, now.AddMinutes(-4), "theirs"),
            Event(recipientId, now.AddMinutes(-2), "middle"));

        // Act
        var events = await _sut.GetBatchEventsAsync(recipientId, 10, ct: CancellationToken.None);

        // Assert
        events.Select(e => e.Title).Should().Equal("oldest", "middle", "newest");
    }

    [Fact]
    public async Task GetBatchEventsAsync_Given_A_Cursor_Should_Continue_After_It()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var timestamp = DateTime.UtcNow;

        await SeedEventsAsync(Enumerable.Range(0, 4)
            .Select(i => Event(recipientId, timestamp, $"event {i}"))
            .ToArray());

        // Act
        var first = await _sut.GetBatchEventsAsync(recipientId, 2, ct: CancellationToken.None);
        var last = first.Last();
        var second = await _sut.GetBatchEventsAsync(recipientId, 2, new PageCursor(last.Created, last.Id), CancellationToken.None);

        // Assert
        first.Should().HaveCount(2);
        second.Should().HaveCount(2);
        first.Select(e => e.Id).Should().NotIntersectWith(second.Select(e => e.Id));
    }

    [Fact]
    public async Task Data_Should_Round_Trip_Through_Jsonb()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        await SeedEventsAsync(Event(recipientId, DateTime.UtcNow, data: """{"tag":"friend#1234","isSeen":false}"""));

        // Act
        var stored = (await _sut.GetBatchEventsAsync(recipientId, 1, ct: CancellationToken.None)).Single();

        // Assert
        using var json = System.Text.Json.JsonDocument.Parse(stored.Data);
        json.RootElement.GetProperty("tag").GetString().Should().Be("friend#1234");
        json.RootElement.GetProperty("isSeen").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task DeleteRange_Should_Remove_Only_The_Given_Events()
    {
        // Arrange
        var recipientId = await SeedRecipientAsync();
        var now = DateTime.UtcNow;

        await SeedEventsAsync(
            Event(recipientId, now.AddMinutes(-2), "first"),
            Event(recipientId, now.AddMinutes(-1), "second"));

        var batch = await _sut.GetBatchEventsAsync(recipientId, 1, ct: CancellationToken.None);

        // Act
        _sut.DeleteRange(batch);
        await _uow.SaveChangesAsync(CancellationToken.None);

        // Assert
        var remaining = await Db.Event.AsNoTracking().ToListAsync();
        remaining.Should().ContainSingle();
        remaining.Single().Title.Should().Be("second");
    }
}
