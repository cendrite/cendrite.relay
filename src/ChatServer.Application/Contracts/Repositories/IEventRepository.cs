using ChatServer.Domain.Entities;

namespace ChatServer.Application.Contracts.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<IReadOnlyCollection<Event>> GetBatchEventsAsync(
        Guid recipientId,
        int batchCount,
        PageCursor? after = null,
        CancellationToken ct = default);

    void DeleteRange(IEnumerable<Event> events);
}
