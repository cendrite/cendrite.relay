using ChatServer.API.Clients;
using ChatServer.API.Common;
using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Connect;
using ChatServer.Application.Features.Client.Commands.Disconnect;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.API.Hubs;

public class BaseHub<T>(
    ISender sender,
    ILogger<BaseHub<T>> logger)
    : Hub<T> where T : class, IClient
{
    public async override Task OnConnectedAsync()
    {
        var deviceId = GetDeviceId();
        ValidateDeviceId(deviceId);

        logger.LogInformation("OnConnectedAsync triggered for connection: {ConnectionId}", Context.ConnectionId);

        var result = await sender.Send(new ConnectClientCommand
        {
            ClientId = deviceId,
            TransientId = Context.ConnectionId,
            HubType = GetHubType()
        });

        if (result.Success is false)
        {
            logger.LogWarning("ConnectClientCommand failed for client: {ClientId}", deviceId);
            Context.Abort();
            return;
        }
        logger.LogInformation("Clients connected: {ClientId}, TransientId: {TransientId}",
            deviceId, Context.ConnectionId);

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

    private Guid GetDeviceId()
    {
        var ctx = Context.GetHttpContext() ?? throw new InvalidOperationException("HTTP Context is null.");
        return ctx.User.GetDeviceId();
    }

    private void ValidateDeviceId(Guid deviceId)
    {
        if (deviceId == Guid.Empty)
        {
            logger.LogError("DeviceId claim is missing or invalid.");
            Context.Abort();
            throw new InvalidOperationException("Invalid or missing DeviceId in claims.");
        }
    }

}
