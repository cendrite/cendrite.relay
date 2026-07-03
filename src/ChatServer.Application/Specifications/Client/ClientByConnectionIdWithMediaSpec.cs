
using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientByConnectionIdWithMediaSepc: Specification<Domain.Aggregates.Client>
{
    public ClientByConnectionIdWithMediaSepc (Guid connectionId) : base(c => c.ConnectionId == connectionId)
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Media)}");
    }
}
