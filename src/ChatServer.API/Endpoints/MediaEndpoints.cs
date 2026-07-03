using ChatServer.API.Common;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.Media.Commands.Create;
using ChatServer.Application.Features.Media.Commands.Update;
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
                [FromQuery] string mediaId,
                [FromQuery] Guid senderId,
                HttpRequest httpreq,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                HttpContext ctx,
                CancellationToken ct) =>
        {
            try
            {
                await mediaSvc.StoreAsync(mediaId, httpreq.Body, ct);
                var result = await sender.Send(new CreateMediaCommand { ClientId = ctx.User.GetUserId(), MediaId = mediaId, SenderId = senderId }, ct);

                return result.Success ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to store media with id {MediaId} for user {UserId}", mediaId, ctx.User.Identity?.Name);
                return Results.Problem("Failed to store media", statusCode: 500);
            }
        });

        group.MapGet("/", async (
                [FromQuery] string id,
                [FromQuery] Guid senderId,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                CancellationToken ct) =>
        {
            try
            {
                var result = await sender.Send(new UpdateMediaCommand { MediaId = id, SenderId = senderId }, ct);
                if (!result.Success)
                    return result.ToProblemDetails();

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
