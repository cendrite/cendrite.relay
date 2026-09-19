using ChatServer.Application.Abstractions;
using ChatServer.Application.Features.Messaging.Models;

namespace ChatServer.Application.Features.Messaging.Commands.Send;

public sealed record SendMessageCommand : ICommand<Receipt>
{
    public required IReadOnlyCollection<IngoingMessage> Bundle { get; init; }
    public required Guid SenderId { get; init; }
}
