using ChatServer.API.Common;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.Media.Commands.Create;
using MediatR;
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
                HttpRequest httpreq,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                HttpContext ctx,
                CancellationToken ct) =>
        {
            try
            {
                var result = await sender.Send(new CreateMediaCommand { ClientId = ctx.User.GetDeviceId() }, ct);
                if (!result.Success)
                    return result.ToProblemDetails();

                await mediaSvc.StoreAsync(result.RequiredValue, httpreq.Body, ct);

                return Results.Ok(new { MediaId = result.Value });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to store media for user {UserId}", ctx.User.Identity?.Name);
                return Results.Problem("Failed to store media", statusCode: 500);
            }
        });

        group.MapGet("/", async (
                [FromQuery] string id,
                IMediaService mediaSvc,
                ILogger<MediaEndpoints> logger,
                CancellationToken ct) =>
        {
            if (!MediaIdValidator.IsValid(id))
            {
                return Results.Problem("Invalid media id", statusCode: 400);
            }

            try
            {
                var stream = mediaSvc.Get(id);
                return Results.File(stream, "application/octet-stream", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve media with id {MediaId}", id);
                return Results.Problem("Failed to retrieve media", statusCode: 500);
            }
        });
    }
}
