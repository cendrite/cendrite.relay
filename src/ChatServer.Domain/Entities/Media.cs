using ChatServer.Domain.Abstractions;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace ChatServer.Domain.Entities;

public class Media : Entity 
{
    public string MediaId { get; private set; }
    public Guid ClientId { get; private set; }

    public Media() { }

    public Media(string mediaId, Guid clientId) : base(Guid.Empty)
    {
        MediaId = mediaId;
        ClientId = clientId;
    }
}
