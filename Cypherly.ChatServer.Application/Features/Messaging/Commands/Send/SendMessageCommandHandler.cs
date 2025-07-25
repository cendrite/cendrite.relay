using Cypherly.ChatServer.Application.Abstractions;
using Cypherly.ChatServer.Application.Cache.Client;
using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Contracts.Cache;
using Cypherly.ChatServer.Application.Enums;
using Cypherly.ChatServer.Application.Features.Messaging.Models;
using Cypherly.ChatServer.Domain.Common;
using Cypherly.ChatServer.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Cypherly.ChatServer.Application.Features.Messaging.Commands.Send;

public sealed class SendMessageCommandHandler(
    IClientCache clientCache,
    IMessageCache messageCache,
    IMessageSender messageSender,
    ILogger<SendMessageCommandHandler> logger)
    : ICommandHandler<SendMessageCommand, SendMessageCommandDto>
{
    public async Task<Result<SendMessageCommandDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var workItems = request.Bundle.Select(WorkItem.BuildItem).ToList();

            logger.LogInformation("Processing {Count} incoming messages", workItems.Count);

            var receipt = await ProcessMessages(workItems, cancellationToken);

            return Result.Ok(new SendMessageCommandDto { Receipt = receipt });
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception occured while sending message");
            return Result.Ok(new SendMessageCommandDto { Receipt = MessageReceipt.Failed });
        }
    }

    private async Task<MessageReceipt> ProcessMessages(IReadOnlyCollection<WorkItem> workItems, CancellationToken ct)
    {
        await Task.WhenAll(workItems.Select(async item =>
        {
            item.Recipient = await clientCache.GetAsync(item.ChatMessage.RecipientId, ct);
            if (item.Recipient?.GetTransientId(HubType.Message) is null)
            {
                logger.LogWarning("Recipient not found for message {MessageId}", item.ChatMessage.Id);
            }

            logger.LogInformation("found recipient: {Recipient}", item.Recipient);
        }));

        var actions = workItems.Select(item => item.Recipient is null
            ? messageCache.CacheMessageAsync(item.ChatMessage, ct)
            : messageSender.SendMessageAsync(item.Recipient.TransientIds[HubType.Message], item.OutgoingMessage, ct));

        await Task.WhenAll(actions);

        return workItems.All(item => item.Recipient is not null)
            ? MessageReceipt.Delivered
            : MessageReceipt.Sent;
    }

    sealed internal class WorkItem
    {
        public required OutgoingMessage OutgoingMessage { get; init; }
        public required ChatMessage ChatMessage { get; init; }
        public ClientCacheDto? Recipient { get; set; }

        public static WorkItem BuildItem(IngoingMessage message)
        {
            return new WorkItem
            {
                OutgoingMessage = message.MapToOutgoingMessage(),
                ChatMessage = message.MapToChatMessage()
            };
        }
    }
}