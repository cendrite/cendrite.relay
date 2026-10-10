
using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Entities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class Message : Entity
{
    public Guid SenderId { get; init; }
    public Guid RecipientId { get; init; }
    public string? MessageToken { get; init; }
    public string Content { get; init; } 
    public DateTime Timestamp { get; init; }

    public Message() { }

    public Message(
        Guid id, 
        Guid senderId, 
        Guid recipientId, 
        string? messageToken, 
        string content, 
        DateTime timestamp) : base(id)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        MessageToken = messageToken;
        Content = content;
        Timestamp = timestamp.ToUniversalTime();
    }
}
