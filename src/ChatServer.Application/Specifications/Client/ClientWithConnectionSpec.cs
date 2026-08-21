using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientWithConnectionSpec : Specification<Domain.Aggregates.Client>
{
    public ClientWithConnectionSpec(Guid connectionID) : base(c => c.ConnectionId == connectionID)
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Connections)}");
    }
}
