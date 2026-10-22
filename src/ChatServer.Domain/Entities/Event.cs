using ChatServer.Domain.Abstractions;

public sealed class Event : Entity 
{
    public Guid ClientId { get; private set; }
    public string EventType { get; private set; }
    public string EventData { get; private set; }
    public DateTime Timestamp { get; private set; }

    public Event() { }

    public Event(Guid clientId, string eventType, string eventData) : base(Guid.Empty)
    {
        ClientId = clientId;
        EventType = eventType;
        EventData = eventData;
        Timestamp = DateTime.UtcNow;
    }

}
