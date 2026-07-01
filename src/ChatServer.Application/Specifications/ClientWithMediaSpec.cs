using ChatServer.Application.Abstractions;
using ChatServer.Domain.Aggregates;

namespace ChatServer.Application.Specifications;

internal sealed class ClientWithMediaSpec : Specification<Client>
{
    public ClientWithMediaSpec(Guid clientId) : base(c => c.Id == clientId)
    {
        AddIncludes($"{nameof(Client.Media)}");
    }
}
