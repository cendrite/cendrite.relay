using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;

public interface IMessageRepository : IRepository<ChatMessage>
{
    Task<IReadOnlyCollection<ChatMessage>> GetBatchMessagesAsync(Guid recipientId, int batchCount, CancellationToken ct = default);
    void DeleteRange(IEnumerable<ChatMessage> messages);
}
