using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Features.Client.Commands.UploadMedia;

public sealed record UploadMediaCommand : ICommand
{
    public required Guid ClientId { get; init; }
    public required string MediaId { get; init; }
}
