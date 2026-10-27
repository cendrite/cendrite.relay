using ChatServer.API.Common;
using ChatServer.Application.Features.Client.Queries.IsRevoked;
using ChatServer.Domain.Common;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.API.Hubs.Filters;

public sealed class RevokedClientHubFilter(ILogger<RevokedClientHubFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var context = invocationContext.Context;
        var deviceId = context.User?.GetDeviceId() ?? throw new HubException(Error.Unauthorized().Code);

        var sender = invocationContext.ServiceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(new IsClientRevokedQuery { ClientId = deviceId }, context.ConnectionAborted);

        if (result.Success is false)
            throw new HubException(Error.Failure().Code);

        if (result.Value)
        {
            logger.LogWarning("Revoked client {ClientId} invoked {Method}, aborting connection {ConnectionId}",
                deviceId, invocationContext.HubMethodName, context.ConnectionId);
            context.Abort();
            throw new HubException(Error.Revoked().Code);
        }

        return await next(invocationContext);
    }
}
