using ChatServer.Application.Contracts.Cache;
using ChatServer.Domain.Entities;
using StackExchange.Redis;

namespace ChatServer.Infrastructure.Caching;

public sealed class MessageCacheService(
    IValkeyCacheService valkeyCacheService,
    IConnectionMultiplexer valkey)
    : IMessageCache
{
    private const string GlobalCacheKey = "undelivered-global:";
    private const string RecipientCacheKeyPrefix = "recipient-msg:";
    private const string MessageCacheKeyPrefix = "msg:";
    private readonly IDatabase _db = valkey.GetDatabase();

    /// <summary>
    /// Asynchronously caches a message by its recipient ID and client message ID.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task CacheMessageAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var key = MessageCacheKeyPrefix + message.Id;
        var listKey = RecipientCacheKeyPrefix + message.RecipientId;

        await valkeyCacheService.SetAsync(key, message, cancellationToken, shouldExipire: true, TimeSpan.FromHours(24));
        await _db.ListRightPushAsync(listKey, key);
        await _db.ListRightPushAsync(GlobalCacheKey, key);
    }

    /// <summary>
    /// Fetches cached messages by recipient ID.
    /// </summary>
    /// <param name="recipientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IReadOnlyCollection<ChatMessage>> GetCachedMessagesAsync(string recipientId, CancellationToken cancellationToken)
    {
        var listKey = RecipientCacheKeyPrefix + recipientId;
        var keys = await _db.ListRangeAsync(listKey);
        if (keys.Length == 0) return [];

        var tasks = keys.Select(key => valkeyCacheService.GetAsync<ChatMessage>(key!, null, cancellationToken));

        var messages = await Task.WhenAll(tasks);
        return messages.Where(m => m is not null).ToArray()!;
    }

    /// <summary>
    /// Remove a cached message by its recipient ID and client message ID.
    /// </summary>
    /// <param name="recepientId"></param>
    /// <param name="messageId"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveCachedMessageAsync(Guid recepientId, Guid messageId, CancellationToken cancellationToken)
    {
        var key = MessageCacheKeyPrefix + messageId;
        var listKey = RecipientCacheKeyPrefix + recepientId;

        await valkeyCacheService.RemoveAsync(key, cancellationToken);
        await _db.ListRemoveAsync(listKey, key);
        await _db.ListRemoveAsync(GlobalCacheKey, key);
    }

    /// <summary>
    /// Gets a batch of cached messages.
    /// </summary>
    /// <param name="batchCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<ChatMessage>> GetBatchCachedMessagesAsync(int batchCount, CancellationToken cancellationToken)
    {
        var keys = await _db.ListRangeAsync(GlobalCacheKey, 0, batchCount - 1);
        if (keys.Length == 0) return [];

        var messages = await Task.WhenAll(
            keys.Select(key => valkeyCacheService.GetAsync<ChatMessage>(key!, null, cancellationToken))
        );

        return messages.Where(m => m is not null).ToArray()!;
    }

    /// <summary>
    /// Removes a batch of cached messages by their message tokens.
    /// </summary>
    /// <param name="messageIds"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveBatchCachedMessagesAsync(IEnumerable<Guid> messageIds, CancellationToken cancellationToken)
    {
        foreach (var id in messageIds)
        {
            var key = MessageCacheKeyPrefix + id;

            await valkeyCacheService.RemoveAsync(key, cancellationToken);
            await _db.ListRemoveAsync(GlobalCacheKey, key);
        }
    }
}
