namespace ChatServer.Application.Features.ChangeEvent.Payloads
{
    public sealed record UserLoggedInEvent
    {
        public required Guid ConnectionId { get; init; }
        public required string UserTag { get; init; }
    }
}
