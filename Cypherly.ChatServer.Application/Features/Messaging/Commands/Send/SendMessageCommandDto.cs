
namespace Cypherly.ChatServer.Application.Features.Messaging.Commands.Send;

public sealed record SendMessageCommandDto
{
    public required MessageReceipt Receipt { get; init; }
}