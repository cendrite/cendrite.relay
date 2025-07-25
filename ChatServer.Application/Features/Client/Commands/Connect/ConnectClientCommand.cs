using ChatServer.Application.Abstractions;
using ChatServer.Application.Enums;

namespace ChatServer.Application.Features.Client.Commands.Connect;

public sealed record ConnectClientCommand : ICommand
{
    public required Guid ClientId { get; init; }
    public required string TransientId { get; init; }
    public required HubType HubType { get; init; }
}