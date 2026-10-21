using ChatServer.Domain.Entities;

namespace ChatServer.Application.Features.Messaging.Models;

/// <summary>
/// Model representing an incoming message from a sender.
/// </summary>
public sealed class IngoingMessage
{
    public required Guid Id { get; init; }
    public required Guid RecipientId { get; init; }
    public required string Payload { get; init; }

    public Message MapToChatMessage(Guid senderId)
    {
        return new Message(
            id: Id,
            senderId: senderId,
            recipientId: RecipientId,
            content: Payload);
    }

    public OutgoingMessage MapToOutgoingMessage(Guid senderId)
    {
        return new OutgoingMessage
        {
            Id = Id,
            SenderId = senderId,
            RecipientId = RecipientId,
            Payload = Payload,
        };
    }
}
