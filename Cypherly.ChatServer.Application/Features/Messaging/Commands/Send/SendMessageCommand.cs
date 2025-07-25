using Cypherly.ChatServer.Application.Abstractions;
using Cypherly.ChatServer.Application.Features.Messaging.Models;

namespace Cypherly.ChatServer.Application.Features.Messaging.Commands.Send;

public sealed record SendMessageCommand : ICommand<SendMessageCommandDto>
{
    public required IReadOnlyCollection<IngoingMessage> Bundle { get; init; }
}