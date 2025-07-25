using Cypherly.ChatServer.Application.Features.Messaging.Models;

namespace Cypherly.ChatServer.API.Clients;

public interface IChatClient : IClient
{
    Task ReceiveMessage(OutgoingMessage message);
    Task ReceiveTypingNotification(string userId);
    Task ReceiveReadReceipt(string messageId, string userId);
}