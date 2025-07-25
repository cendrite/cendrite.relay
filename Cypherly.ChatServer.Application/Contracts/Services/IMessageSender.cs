using Cypherly.ChatServer.Application.Features.Messaging.Models;

namespace Cypherly.ChatServer.Application.Contracts;

public interface IMessageSender
{
    Task SendMessageAsync(string transientId, OutgoingMessage message, CancellationToken cancellationToken = default);
}