using Cypherly.ChatServer.API.Clients;
using Cypherly.ChatServer.API.Hubs;
using Cypherly.ChatServer.Application.Contracts;
using Cypherly.ChatServer.Application.Features.ChangeEvent.Base;
using Microsoft.AspNetCore.SignalR;

namespace Cypherly.ChatServer.API.Handlers;

public class ChangeEventHandler(IHubContext<ChangeEventHub, IChangeEventClient> hubContext) : IChangeEventNotifier
{
    public async Task NotifyAsync(string transientId, ChangeEvent changeEvent, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Client(transientId).ChangeNotification(changeEvent);
    }
}