
using ChatServer.Application.Abstractions;
using ChatServer.Application.Enums;

namespace ChatServer.Application.Features.Client.Commands.Disconnect;

public sealed record DisconnectClientCommand : ICommand
{
    public required string TransientId { get; init; }
    public required HubType HubType { get; init; }
}