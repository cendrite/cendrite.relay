using ChatServer.Domain.Entities;

namespace ChatServer.Application.Contracts.Cache;

public interface IMessageCache
{
    public Task CacheMessageAsync(ChatMessage message, CancellationToken ct = default);
    public Task<IReadOnlyCollection<ChatMessage>> GetCachedMessagesAsync(string recipientId, CancellationToken ct = default);
    public Task RemoveCachedMessageAsync(Guid recepientId, Guid messageId, CancellationToken ct = default);
    public Task<IReadOnlyCollection<ChatMessage>> GetBatchCachedMessagesAsync(int batchCount, CancellationToken ct = default);
    public Task RemoveBatchCachedMessagesAsync(IEnumerable<Guid> messageIds, CancellationToken ct = default);
}
