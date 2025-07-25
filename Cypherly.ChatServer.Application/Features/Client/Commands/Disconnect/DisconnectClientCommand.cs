
using Cypherly.ChatServer.Application.Abstractions;

namespace Cypherly.ChatServer.Application.Features.Client.Commands.Disconnect;

public sealed record DisconnectClientCommand : ICommand
{
    public required string TransientId { get; init; }
}