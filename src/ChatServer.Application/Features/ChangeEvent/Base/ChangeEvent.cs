using System.Text.Json;
using ChatServer.Domain.Entities;

namespace ChatServer.Application.Features.ChangeEvent.Base;

public class ChangeEvent(Guid id, ChangeEventType type, string title, string description, object data)
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; private init; } = id;

    /// <summary>
    /// The type of the event
    /// </summary>
    public ChangeEventType Type { get; private set; } = type;

    /// <summary>
    /// A short, human-readable title for the event
    /// </summary>
    public string Title { get; private set; } = title;

    /// <summary>
    /// Short description of the event
    /// </summary>
    public string Description { get; private set; } = description;

    /// <summary>
    /// When the event occured
    /// </summary>
    public DateTime OccurredOn { get; private init; } = DateTime.UtcNow;

    /// <summary>
    /// Payload containing the data of the event
    /// </summary>
    public object Data { get; private set; } = data;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Maps the event to a <see cref="Event"/> so it can be held until the recipient connects
    /// </summary>
    public Event ToEvent(Guid recipientId) => new(
        Id,
        recipientId,
        Type.ToString(),
        Title,
        Description,
        OccurredOn,
        JsonSerializer.Serialize(Data, Data.GetType(), SerializerOptions));

    /// <summary>
    /// Restores a held event. The payload is kept as raw JSON, so it reaches the client in the same shape it was stored in.
    /// </summary>
    public static ChangeEvent FromEvent(Event evt) => new(
        evt.Id,
        Enum.Parse<ChangeEventType>(evt.Type),
        evt.Title,
        evt.Description,
        JsonSerializer.Deserialize<JsonElement>(evt.Data, SerializerOptions))
    {
        OccurredOn = evt.OccurredOn
    };
}
