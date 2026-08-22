using ChatServer.Application.Abstractions;
using ChatServer.Application.Contracts.Cache;
using ChatServer.Application.Contracts.Repositories;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Messaging.Models;
using ChatServer.Application.Specifications.Client;
using ChatServer.Domain.Common;
using ChatServer.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.Features.Messaging.Commands.Send;

public sealed class SendMessageCommandHandler(
    IClientRepository clientRepo,
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
            logger.LogDebug("Messages: {Messages}", workItems.Select(wi => wi.ChatMessage));

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
        foreach (var item in workItems)
        {
            item.Recipient = await clientRepo.GetAsync(new ClientWithConnectionSpec(item.ChatMessage.RecipientId), ct);
            if (item.Recipient?.GetConnection(HubType.Message.ToString()) is null)
            {
                logger.LogWarning("Recipient not found for message {MessageId}", item.ChatMessage.Id);
            }

            logger.LogInformation("found recipient: {Recipient}", item.Recipient);
        }

        var actions = workItems.Select(item => item.Recipient?.GetConnection(HubType.Message.ToString())?.TransientId is { } messageTransientId
            ? messageSender.SendMessageAsync(messageTransientId, item.OutgoingMessage, ct)
            : messageCache.CacheMessageAsync(item.ChatMessage, ct));

        await Task.WhenAll(actions);

        return workItems.All(item => item.Recipient?.GetConnection(HubType.Message.ToString())?.TransientId is not null)
            ? MessageReceipt.Delivered
            : MessageReceipt.Sent;
    }

    sealed internal class WorkItem
    {
        public required OutgoingMessage OutgoingMessage { get; init; }
        public required ChatMessage ChatMessage { get; init; }
        public Domain.Aggregates.Client? Recipient { get; set; }

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
