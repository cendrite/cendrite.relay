using ChatServer.API.Clients;
using ChatServer.Application.Features.Messaging.Commands.Send;
using ChatServer.Application.Features.Messaging.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ChatServer.API.Hubs;

[Authorize]
public class MessageHub(
    ISender sender,
    ILogger<BaseHub<IChatClient>> logger)
    : BaseHub<IChatClient>(sender, logger)
{
    private readonly ISender _sender = sender;

    public async Task<Receipt> SendMessage(IReadOnlyCollection<IngoingMessage> ingoingMessage)
    {
        var senderId = GetDeviceId();
        var result = await _sender.Send(new SendMessageCommand { Bundle = ingoingMessage, SenderId = senderId });
        return result.Success ? result.Value : Receipt.Failed;
    }
}
