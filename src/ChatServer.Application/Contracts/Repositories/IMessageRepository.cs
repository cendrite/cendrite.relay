using ChatServer.Application.Contracts.Repositories;
using ChatServer.Domain.Entities;

public interface IMessageRepository : IRepository<Message>
{
    /// <summary>
    /// Returns the next page of a recipient's undelivered messages, oldest first.
    /// </summary>
    /// <param name="after">Cursor from the last message of the previous page, or null for the first page.</param>
    Task<IReadOnlyCollection<Message>> GetBatchMessagesAsync(
        Guid recipientId,
        int batchCount,
        MessageCursor? after = null,
        CancellationToken ct = default);

    void DeleteRange(IEnumerable<Message> messages);
}
