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
                HttpRequest httpreq,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                HttpContext ctx,
                CancellationToken ct) =>
        {
            if (!MediaIdValidator.IsValid(mediaId))
            {
                return Results.Problem("Invalid media id", statusCode: 400);
            }

            try
            {
                await mediaSvc.StoreAsync(mediaId, httpreq.Body, ct);
                var result = await sender.Send(new CreateMediaCommand { ClientId = ctx.User.GetDeviceId(), MediaId = mediaId }, ct);

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

        group.MapPost("{id}/ack", async (
                [FromRoute] string id,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateMediaCommand { MediaId = id }, ct);
            return result.Success ? Results.Ok() : result.ToProblemDetails();
        });
    }
}
