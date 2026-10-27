using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientWithConnectionsByIdSpec : Specification<Domain.Aggregates.Client>
{
    public ClientWithConnectionsByIdSpec(Guid clientId) : base(c => c.Id == clientId)
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Connections)}");
    }
}
