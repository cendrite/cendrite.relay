using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Entities;

public class ChatMessage : Entity
{
    public string ChatId { get; init; } = null!;
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public Guid MessageToken { get; init; }
    public string Content { get; init; } = null!;
    public DateTime Timestamp { get; init; }

    public ChatMessage() { }

    public ChatMessage(Guid id, Guid senderId, Guid recipientId, string chatId, Guid messageToken, string content) : base(id)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        ChatId = chatId;
        MessageToken = messageToken;
        Content = content;
    }
}
