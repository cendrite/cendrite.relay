using ChatServer.Domain.Entities;

namespace ChatServer.Application.Contracts.Repositories;

public interface IEventRepository : IRepository<Event>
{
    /// <summary>
    /// Returns the next page of a recipient's undelivered change events, oldest first.
    /// </summary>
    /// <param name="after">Cursor from the last event of the previous page, or null for the first page.</param>
    Task<IReadOnlyCollection<Event>> GetBatchEventsAsync(
        Guid recipientId,
        int batchCount,
        PageCursor? after = null,
        CancellationToken ct = default);

    void DeleteRange(IEnumerable<Event> events);
}
