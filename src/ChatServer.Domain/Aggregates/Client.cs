using ChatServer.Domain.Abstractions;
using ChatServer.Domain.Entities;

namespace ChatServer.Domain.Aggregates;

public class Client : AggregateRoot
{
    /// <summary>
    /// ConnectionId is used send msg between clients and serves as a stable identifier for the SignalR connection.
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// TransientId is used to store the SignalR connectionId for caching purposes.
    /// Will not be persisted to the database.
    /// </summary>
    public string? TransientId { get; private set; }

    private readonly List<Media> _media = [];
    private readonly List<ChatMessage> _chatMessages = [];
    private readonly List<Connection> _connections = [];

    public IReadOnlyCollection<ChatMessage> ChatMessages => _chatMessages;
    public IReadOnlyCollection<Media> Media => _media;
    public IReadOnlyCollection<Connection> Connections => _connections;

    private Client() : base(Guid.Empty) { } // For EF Core

    public Client(Guid id, Guid connectionId) : base(id)
    {
        ConnectionId = connectionId;
    }

    public void SetTransientId(string transientId) => TransientId = transientId;

    public void AddMedia(Media media)
    {
        if (_media.Any(m => m.MediaId == media.MediaId))
            return;

        _media.Add(media);
    }

    public void AddConnection(Connection connection)
    {

        if (_connections.Any(c => c.TransientId == connection.TransientId))
            return;

        _connections.Add(connection);
    }

    public void RemoveConnection(Connection connection) => _connections.Remove(connection);

    public void RemoveMedia(Media media) => _media.Remove(media);

    public Media? GetMedia(string mediaId)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
            throw new ArgumentException("MediaId cannot be null or whitespace.", nameof(mediaId));

        var media = _media.FirstOrDefault(m => m.MediaId == mediaId);

        return media;
    }
}
