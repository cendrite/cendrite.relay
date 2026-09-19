using ChatServer.Domain.Entities;

namespace ChatServer.Application.Features.Messaging.Models;

/// <summary>
/// Model representing an incoming message from a sender.
/// </summary>
public sealed class IngoingMessage
{
    public required Guid Id { get; init; }
    public required string ChatId { get; init; }
    public required Guid RecipientId { get; init; }
    public required string Payload { get; init; }
    public string? MessageToken { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// SenderId is intentionally not a property on this model - it must be derived from the
    /// authenticated connection (see SendMessageCommand.SenderDeviceId), never trusted from the client payload.
    /// </summary>
    public ChatMessage MapToChatMessage(Guid senderId)
    {
        return new ChatMessage(
            id: Id,
            senderId: senderId,
            recipientId: RecipientId,
            chatId: ChatId,
            messageToken: MessageToken,
            content: Payload);
    }

    public OutgoingMessage MapToOutgoingMessage(Guid senderId)
    {
        return new OutgoingMessage
        {
            Id = Id,
            SenderId = senderId,
            RecipientId = RecipientId,
            ChatId = ChatId,
            Payload = Payload,
            Timestamp = Timestamp
        };
    }
}
