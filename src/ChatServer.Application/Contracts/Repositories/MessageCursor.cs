namespace ChatServer.Application.Contracts.Repositories;

public readonly record struct MessageCursor(DateTime Created, Guid Id);
