using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client;

internal sealed class ClientWithMediaSpec : Specification<Domain.Aggregates.Client>
{
    public ClientWithMediaSpec(Guid clientId) : base(c => c.Id == clientId)
    {
        AddIncludes($"{nameof(Domain.Aggregates.Client.Media)}");
    }
}
