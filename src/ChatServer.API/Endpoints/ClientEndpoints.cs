using ChatServer.API.Common;
using ChatServer.Application.Features.Client.Queries.GetPresence;
using Mediator;

internal sealed class ClientEndpoints : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder rtb)
    {
        var group = rtb.MapGroup("api/client")
            .WithTags("Client")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/presence", async (
                ISender sender,
                HttpContext ctx,
                CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPresenceQuery { UserId = ctx.User.GetUserId() }, ct);
            return result.Success ? Results.Ok(result.Value) : result.ToProblemDetails();
        })
        .Produces<GetPresenceDto>()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
