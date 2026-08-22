using ChatServer.Application.Contracts.Cache;
using ChatServer.Domain.Entities;
using ChatServer.Infrastructure.Interfaces;
using ChatServer.Infrastructure.Persistence.Context;
using ChatServer.Test.Integration.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Test.Integration.Cache;

public class
    MessageCacheServiceTest : IntegrationTestbase
{
    private readonly IValkeyCacheService _cache;
    private readonly IMessageCache _sut;

    public MessageCacheServiceTest(IntegrationTestFactory<Program, ChatServerDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _cache = scope.ServiceProvider.GetRequiredService<IValkeyCacheService>();
        _sut = scope.ServiceProvider.GetRequiredService<IMessageCache>();
    }

    [Fact]
    public async Task CacheMessageAsync_Should_CacheMessage()
    {
        // Arrange
        var message = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret chat message");

        // Act
        await _sut.CacheMessageAsync(message, CancellationToken.None);

        // Assert
        var cachedMessage = await _sut.GetCachedMessagesAsync(message.RecipientId.ToString(), CancellationToken.None);

        cachedMessage.First().Should().NotBeNull();
        cachedMessage.First().Should().BeEquivalentTo(message);
        cachedMessage.First().MessageToken.Should().Be(message.MessageToken);
        cachedMessage.First().RecipientId.Should().Be(message.RecipientId);
    }

    [Fact]
    public async Task GetCachedMessagesAsync_GivenValidRecipientId_ShouldReturnCachedMessages()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var message1 = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), recipientId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret chat message 1");
        var message2 = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), recipientId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret chat message 1");
        await _sut.CacheMessageAsync(message1, CancellationToken.None);
        await _sut.CacheMessageAsync(message2, CancellationToken.None);

        // Act
        var cachedMessages = await _sut.GetCachedMessagesAsync(recipientId.ToString(), CancellationToken.None);

        // Assert
        cachedMessages.Should().NotBeNullOrEmpty();
        cachedMessages.Should().HaveCount(2);
        cachedMessages.Should().Contain(m => m.MessageToken == message1.MessageToken);
        cachedMessages.Should().Contain(m => m.MessageToken == message2.MessageToken);
    }

    [Fact]
    public async Task RemoveCachedMessageAsync_Should_Remove_CachedMessage()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var message = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), recipientId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "secret chat message");
        await _sut.CacheMessageAsync(message, CancellationToken.None);

        // Act
        await _sut.RemoveCachedMessageAsync(recipientId, message.Id, CancellationToken.None);

        // Assert
        var cachedMessage = await _cache.GetAsync<ChatMessage>("msg:" + message.Id, null, CancellationToken.None);
        cachedMessage.Should().BeNull();
        var cachedMessages = await _sut.GetCachedMessagesAsync(recipientId.ToString(), CancellationToken.None);
        cachedMessages.Should().NotContain(m => m.MessageToken == message.MessageToken);
    }

    [Fact]
    public async Task GetBatchedCachedMessagesAsync_Should_GetBatchedCachedMessages()
    {
        // Arrange
        for (var i = 0; i < 100; i++)
        {
            var message = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), $"secret chat message {i}");
            await _sut.CacheMessageAsync(message, CancellationToken.None);
        }

        // Act
        var cachedMessages = await _sut.GetBatchCachedMessagesAsync(50, CancellationToken.None);

        // Assert
        cachedMessages.Should().NotBeNullOrEmpty();
        cachedMessages.Should().HaveCount(50);
        cachedMessages.Should().OnlyHaveUniqueItems(m => m.MessageToken);
    }

    [Fact]
    public async Task RemoveBatchCachedMessagesAsync_Should_RemoveBatchedCachedMessages()
    {
        // Arrange
        var messages = new List<ChatMessage>();
        for (var i = 0; i < 100; i++)
        {
            var message = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), $"secret chat message {i}");
            messages.Add(message);
            await _sut.CacheMessageAsync(message, CancellationToken.None);
        }

         //Act
        await _sut.RemoveBatchCachedMessagesAsync(messages.Select(x => x.Id), CancellationToken.None);

        // Assert
        var cachedMessages = await _sut.GetBatchCachedMessagesAsync(100, CancellationToken.None);

        foreach (var message in messages)
        {
            cachedMessages.Should().NotContain(m => m.MessageToken == message.MessageToken);
        }
    }
}
