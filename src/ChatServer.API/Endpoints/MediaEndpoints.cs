using ChatServer.API.Common;
using ChatServer.Application.Contracts.Services;
using ChatServer.Application.Features.Client.Commands.UploadMedia;
using ChatServer.Application.Features.Media.Commands.UploadMedia;
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
                [FromQuery] string id,
                HttpRequest req,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger,
                HttpContext ctx,
                CancellationToken ct) =>
        {
            try
            {
                await mediaSvc.StoreAsync(id, req.Body, ct);
                var result = await sender.Send(new CreateMediaCommand { ClientId = ctx.User.GetUserId(), MediaId = id }, ct);

                return result.Success ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to store media with id {MediaId} for user {UserId}", id, ctx.User.Identity?.Name);
                return Results.Problem("Failed to store media", statusCode: 500);
            }
        });

        group.MapGet("/{id}", async (
                [FromRoute] string id,
                IMediaService mediaSvc,
                ISender sender,
                ILogger<MediaEndpoints> logger) =>
        {
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
