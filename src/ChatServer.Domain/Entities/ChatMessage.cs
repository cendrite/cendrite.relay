using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Entities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class ChatMessage : Entity
{
    public string ChatId { get; private set; } 
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public Guid MessageToken { get; private set; }
    public string Content { get; private set; } 
    public DateTime Timestamp { get; private set; }

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
