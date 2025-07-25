using Cypherly.ChatServer.Domain.Entities;

namespace Cypherly.ChatServer.Application.Contracts.Cache;

public interface IMessageCache
{
    public Task CacheMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<ChatMessage>> GetCachedMessagesAsync(string recipientId, CancellationToken cancellationToken = default);
    public Task RemoveCachedMessageAsync(Guid recepientId, Guid messageId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<ChatMessage>> GetBatchCachedMessagesAsync(int batchCount, CancellationToken cancellationToken = default);
    public Task RemoveBatchCachedMessagesAsync(IEnumerable<Guid> messageIds, CancellationToken cancellationToken = default);

}