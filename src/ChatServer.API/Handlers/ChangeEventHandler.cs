using ChatServer.API.Clients;
using ChatServer.API.Hubs;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.ChangeEvent.Base;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.API.Handlers;

public class ChangeEventHandler(IHubContext<ChangeEventHub, IChangeEventClient> hubContext) : IChangeEventNotifier
{
    public async Task NotifyAsync(string transientId, ChangeEvent changeEvent, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Client(transientId).ChangeNotification(changeEvent);
    }
}
