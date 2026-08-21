using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientByTransientIdSpec : Specification<Domain.Aggregates.Client>
{
    public ClientByTransientIdSpec(string transientId) : base(c => c.Connections.Any(conn => conn.TransientId == transientId))
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Connections)}");
    }
}
