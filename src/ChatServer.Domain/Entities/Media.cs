using ChatServer.Domain.Abstractions;
using ChatServer.Domain.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace ChatServer.Domain.Entities;

public sealed class Media : Entity 
{
    public string MediaId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid SenderId { get; private set; }
    public MediaStatus Status { get; private set; } 

    public Media() { }

    public Media(string mediaId, Guid clientId, Guid senderId) : base(Guid.Empty)
    {
        MediaId = mediaId;
        ClientId = clientId;
        SenderId = senderId;
        Status = MediaStatus.Pending;
    }

    public void MarkAsSent() => Status = MediaStatus.Downloaded;

    public void MarkAsFailed() => Status = MediaStatus.Failed;
}
