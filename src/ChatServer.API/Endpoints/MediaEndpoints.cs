using ChatServer.API.Common;
using Microsoft.AspNetCore.Mvc;

internal sealed class MediaEndpoints : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder rtb)
    {
        var group = rtb.MapGroup("api/media")
            .WithTags("Media")
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/upload", async (
                [FromQuery] string id, 
                HttpRequest req, 
                IMediaService mediaSvc, 
                HttpContext httpCtx,
                CancellationToken ct) =>
        {
            await mediaSvc.StoreAsync(id, req.Body, ct);
            return Results.Ok();
        });
    }
}
