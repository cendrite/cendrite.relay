using Cypherly.ChatServer.API.Clients;
using Cypherly.ChatServer.API.Hubs;
using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Features.Messaging.Models;
using Microsoft.AspNetCore.SignalR;

namespace Cypherly.ChatServer.API.Handlers;

public sealed class MessageHandler(IHubContext<MessageHub, IChatClient> hubContext) : IMessageSender
{
    public async Task SendMessageAsync(string transientId, OutgoingMessage message, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Client(transientId).ReceiveMessage(message);
    }
}