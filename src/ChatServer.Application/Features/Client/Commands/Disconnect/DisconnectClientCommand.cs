
using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Client.Commands.Disconnect;

public sealed record DisconnectClientCommand : ICommand
{
    public required string TransientId { get; init; }
}