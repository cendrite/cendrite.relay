using ChatServer.Domain.Abstractions;
using ChatServer.Domain.Enums;

namespace ChatServer.Domain.Entities;

public class ChatMessage : Entity
{
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public Guid ChatId { get; init; }
    public Guid MessageToken { get; init; }
    public string Content { get; init; } = null!;
    public MessageStatus Status { get; set; }
    public ChatMessage() { }

    public ChatMessage(Guid id, Guid senderId, Guid recipientId, Guid chatId, Guid messageToken, string content) : base(id)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        ChatId = chatId;
        MessageToken = messageToken;
        Content = content;
        Status = MessageStatus.Sent;
    }

    public void MarkAsDelivered()
    {
        Status = MessageStatus.Delivered;
    }

    public void MarkAsRead()
    {
        Status = MessageStatus.Read;
    }
}