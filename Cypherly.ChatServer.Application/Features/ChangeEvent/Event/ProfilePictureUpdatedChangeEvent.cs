namespace Cypherly.ChatServer.Application.Features.ChangeEvent.Event;

public sealed record ProfilePictureUpdatedChangeEvent(string UserTag, string ProfilePictureUrl)
{
    public string UserTag { get; private init; } = UserTag;
    public string ProfilePictureUrl { get; private init; } = ProfilePictureUrl;
}