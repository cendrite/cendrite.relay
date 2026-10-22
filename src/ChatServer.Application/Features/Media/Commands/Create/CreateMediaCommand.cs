using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Media.Commands.Create;

public sealed record CreateMediaCommand : ICommand<Guid>
{
    public required Guid ClientId { get; init; }
}
