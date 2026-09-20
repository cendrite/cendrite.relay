namespace ChatServer.Application.Contracts.Repositories;

/// <summary>
/// Keyset cursor for draining a recipient's mailbox. Ordering is (Timestamp, Id) rather than
/// Timestamp alone so messages sharing a timestamp still have a stable, total order — without
/// the tiebreaker a page boundary landing inside a group of equal timestamps would skip or
/// repeat rows.
/// </summary>
public readonly record struct MessageCursor(DateTime Timestamp, Guid Id);
