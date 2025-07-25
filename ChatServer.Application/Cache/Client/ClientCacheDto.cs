using ChatServer.Application.Enums;

namespace ChatServer.Application.Cache.Client;

public class ClientCacheDto
{
    public Guid ConnectionId { get; private set; }
    public Dictionary<HubType, string> TransientIds { get; private set; } = new();

    public ClientCacheDto(Guid connectionId, Dictionary<HubType, string> transientIds)
    {
        ConnectionId = connectionId;
        TransientIds = transientIds;
    }

    public void AddTransientId(string connectionId, HubType hubType)
    {
        TransientIds.Add(hubType, connectionId);
    }
    public void RemoveTransientId(HubType hubType)
    {
        TransientIds.Remove(hubType);
    }

    public string? GetTransientId(HubType hubType)
    {
        return TransientIds.GetValueOrDefault(hubType);
    }

}