using ChatServer.Application.Enums;

namespace ChatServer.Application.Cache.Client;

public class ClientCacheDto(Guid connectionId, Dictionary<HubType, string> transientIds)
{
    public Guid ConnectionId { get; private set; } = connectionId;
    public Dictionary<HubType, string> TransientIds { get; private set; } = transientIds;

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
