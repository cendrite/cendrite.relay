using ChatServer.Domain.Abstractions;

#pragma warning disable IDE0290 // Use primary constructor
public sealed class Connection : Entity
{
    public Guid TransientId { get; private set; }
    public Guid ClientId { get; private set; }
    public string Type { get; private set; }

    public Connection(Guid transientId, Guid clientId, string type) : base(Guid.Empty)
    {
        TransientId = transientId;
        ClientId = clientId;
        Type = type;
    }
}
