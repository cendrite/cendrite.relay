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

    public async Task<MessageReceipt> SendMessage(IReadOnlyCollection<IngoingMessage> ingoingMessage)
    {
        var result = await _sender.Send(new SendMessageCommand { Bundle = ingoingMessage });
        return result.Value!.Receipt;
    }
}
