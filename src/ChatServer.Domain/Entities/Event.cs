using ChatServer.Domain.Abstractions;

namespace ChatServer.Domain.Entities;

/// <summary>
/// A change event held for a recipient whose change event hub was not connected when it occurred.
/// Flushed and removed once the recipient connects.
/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public sealed class Event : Entity
{
    public Guid RecipientId { get; init; }
    public string Type { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTime OccurredOn { get; init; }

    /// <summary>
    /// JSON serialized payload of the event
    /// </summary>
    public string Data { get; init; }

    public Event() { }

    public Event(
        Guid id,
        Guid recipientId,
        string type,
        string title,
        string description,
        DateTime occurredOn,
        string data)
        : base(id)
    {
        RecipientId = recipientId;
        Type = type;
        Title = title;
        Description = description;
        OccurredOn = occurredOn;
        Data = data;
    }
}
