using ChatServer.Application.Abstractions;

namespace ChatServer.Application.Specifications.Client
{
    internal sealed class ClientWithConnectionSpec : Specification<Domain.Aggregates.Client>
    {

        public ClientWithConnectionSpec(Guid connectionID) : base(c => c.Connections.Any(conn => conn.Id == connectionID))
        {
            AddIncludes($"{nameof(Domain.Aggregates.Client.Connections)}");
        }
    }
}
