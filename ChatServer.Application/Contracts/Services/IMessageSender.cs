using ChatServer.Application.Features.Messaging.Models;

namespace ChatServer.Application.Contracts.Services;

public interface IMessageSender
{
    Task SendMessageAsync(string transientId, OutgoingMessage message, CancellationToken cancellationToken = default);
}
