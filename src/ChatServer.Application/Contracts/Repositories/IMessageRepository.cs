using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;

public interface IMessageRepository : IRepository<ChatMessage>
{
    public Task<IReadOnlyCollection<ChatMessage>> GetBatchMessagesAsync(int recipientId, int batchCount, CancellationToken ct = default);
}
