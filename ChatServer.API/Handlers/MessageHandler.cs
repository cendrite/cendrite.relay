using ChatServer.API.Clients;
using ChatServer.API.Hubs;
using ChatServer.Application.Contracts;
using ChatServer.Application.Features.Messaging.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.API.Handlers;

public sealed class MessageHandler(IHubContext<MessageHub, IChatClient> hubContext) : IMessageSender
{
    public async Task SendMessageAsync(string transientId, OutgoingMessage message, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Client(transientId).ReceiveMessage(message);
    }
}