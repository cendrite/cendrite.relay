using ChatServer.Application.Features.Messaging.Models;

namespace ChatServer.API.Clients;

public interface IChatClient : IClient
{
    Task ReceiveMessage(OutgoingMessage message);
    Task ReceiveTypingNotification(string userId);
    Task ReceiveReadReceipt(string messageId, string userId);
}