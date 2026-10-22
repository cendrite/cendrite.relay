using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed record CreateMediaCommand : ICommand<string>
{
    public required Guid ClientId { get; init; }
    public required long Size { get; init; }
}
