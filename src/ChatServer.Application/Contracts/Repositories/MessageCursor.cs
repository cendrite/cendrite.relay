namespace ChatServer.Application.Contracts.Repositories;

public readonly record struct MessageCursor(DateTime Timestamp, Guid Id);
