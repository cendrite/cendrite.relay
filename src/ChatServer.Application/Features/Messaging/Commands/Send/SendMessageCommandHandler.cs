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
    : ICommandHandler<SendMessageCommand, Receipt>
{
    public async Task<Result<Receipt>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sender = await clientRepo.GetByIdAsync(request.SenderDeviceId, cancellationToken);
            if (sender is null)
            {
                logger.LogWarning("Sender client not found for device {DeviceId}", request.SenderDeviceId);
                return Result.Ok(Receipt.Failed);
            }

            var workItems = request.Bundle.Select(m => WorkItem.BuildItem(m, sender.ConnectionId)).ToList();

            var receipt = await ProcessMessages(workItems, cancellationToken);

            return Result.Ok(receipt);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception occured while sending message");
            return Result.Ok(Receipt.Failed);
        }
    }

    private async Task<Receipt> ProcessMessages(IReadOnlyCollection<WorkItem> workItems, CancellationToken ct)
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

        return Receipt.Sent;
    }

    sealed internal class WorkItem
    {
        public required OutgoingMessage OutgoingMessage { get; init; }
        public required ChatMessage ChatMessage { get; init; }
        public Domain.Aggregates.Client? Recipient { get; set; }

        public static WorkItem BuildItem(IngoingMessage message, Guid senderId)
        {
            return new WorkItem
            {
                OutgoingMessage = message.MapToOutgoingMessage(senderId),
                ChatMessage = message.MapToChatMessage(senderId)
            };
        }
    }
}
