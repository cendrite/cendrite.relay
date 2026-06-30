using ChatServer.Domain.Abstractions;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class Media : Entity 
{
    public string MediaId { get; private set; }
    public string SenderId { get; private set; }

    public Media() { }

    public Media(Guid id, string mediaId, string senderId) : base(id)
    {
        MediaId = mediaId;
        SenderId = senderId;
    }
}
