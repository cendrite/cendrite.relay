using System.Security.Claims;
using Cypherly.ChatServer.API.Clients;
using Cypherly.ChatServer.Application.Enums;
using Cypherly.ChatServer.Application.Features.Client.Commands.Connect;
using Cypherly.ChatServer.Application.Features.Client.Commands.Disconnect;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Cypherly.ChatServer.API.Hubs;

public class BaseHub<T>(
    ISender sender,
    ILogger<BaseHub<T>> logger)
    : Hub<T> where T : class, IClient
{
    public async override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var deviceId = httpContext.User.FindFirst("sub")?.Value;

        logger.LogInformation("OnConnectedAsync triggered for connection: {ConnectionId}", Context.ConnectionId);

        if (deviceId != null && Guid.TryParse(deviceId, out var deviceIdGuid))
        {
            var result = await sender.Send(new ConnectClientCommand
            {
                ClientId = deviceIdGuid,
                TransientId = Context.ConnectionId,
                HubType = GetHubType()
            });

            if (result.Success is false)
            {
                logger.LogWarning("ConnectClientCommand failed for client: {ClientId}", deviceIdGuid);
                Context.Abort();
                return;
            }
            logger.LogInformation("Clients connected: {ClientId}, TransientId: {TransientId}",
                deviceIdGuid, Context.ConnectionId);
        }
        else
        {
            logger.LogWarning("Clients connection failed: {ClientId}", deviceId);
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("OnDisconnectedAsync triggered for connection: {ConnectionId}", Context.ConnectionId);
        await sender.Send(new DisconnectClientCommand { TransientId = Context.ConnectionId });
        await base.OnDisconnectedAsync(exception);
    }

    private static HubType GetHubType()
    {
        if (typeof(T) == typeof(IChatClient))
            return HubType.Message;

        if (typeof(T) == typeof(IChangeEventClient))
            return HubType.ChangeEvent;

        throw new InvalidOperationException($"Unsupported hub type for {typeof(T).Name}");
    }
}