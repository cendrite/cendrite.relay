namespace ChatServer.Application.Features.ChangeEvent.Payloads;

public sealed record AvatarUpdatedEvent(string UserTag, Guid AvatarKey)
{
    public string UserTag { get; private init; } = UserTag;
    public Guid AvatarKey { get; private init; } = AvatarKey;
}
