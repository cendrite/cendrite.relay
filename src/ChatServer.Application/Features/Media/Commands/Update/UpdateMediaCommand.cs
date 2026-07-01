using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Media.Commands.Update;

public sealed record UpdateMediaCommand : ICommand
{
    public required string MediaId { get; init; }
}
