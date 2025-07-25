using ChatServer.Domain.Entities;

namespace ChatServer.Application.Features.Messaging.Models;

/// <summary>
/// Model representing an incoming message from a sender.
/// </summary>
public sealed class IngoingMessage
{
    public required Guid Id { get; init; }
    public required Guid ChatId { get; init; }
    public required Guid RecipientId { get; init; }
    public required Guid SenderId { get; init; }
    public required Guid MessageToken { get; init; }
    public required string Content { get; init; } = null!;

    public ChatMessage MapToChatMessage()
    {
        return new ChatMessage(
            id: Id,
            senderId: SenderId,
            recipientId: RecipientId,
            chatId: ChatId,
            messageToken: MessageToken,
            content: Content);
    }

    public OutgoingMessage MapToOutgoingMessage()
    {
        return new OutgoingMessage
        {
            Id = Id,
            SenderId = SenderId,
            RecipientId = RecipientId,
            ChatId = ChatId,
            Content = Content
        };
    }
}