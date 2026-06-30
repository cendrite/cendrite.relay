using ChatServer.Application.Abstractions;

public sealed record AddMediaCommand : ICommand
{
    public required Guid ClientId { get; init; }
    public required string MediaId { get; init; }
}
