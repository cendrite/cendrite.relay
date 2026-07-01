using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed record CreateMediaCommand : ICommand
{
    public required Guid ClientId { get; init; }
    public required string MediaId { get; init; }
}
