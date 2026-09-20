using ChatServer.Domain.Entities;

namespace ChatServer.Application.Features.Messaging.Models;

/// <summary>
/// Model representing an outgoing message to be sent to a recipient.
/// </summary>
public sealed class OutgoingMessage
{
    public required Guid Id { get; init; }
    public required Guid SenderId { get; init; }
    public required Guid RecipientId { get; init; }
    public required string ChatId { get; init; }
    public required string Payload { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    public static OutgoingMessage MapFrom(ChatMessage chatMessage)
    {
        return new OutgoingMessage
        {
            Id = chatMessage.Id,
            SenderId = chatMessage.SenderId,
            RecipientId = chatMessage.RecipientId,
            ChatId = chatMessage.ChatId,
            Payload = chatMessage.Content,
            Timestamp = chatMessage.Timestamp
        };
    }
}
